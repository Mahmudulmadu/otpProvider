using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtpProvider.Application.DTOs;
using OtpProvider.Application.Interfaces;
using OtpProvider.Application.Interfaces.IRepository;
using OtpProvider.Domain.Entities;
using OtpProvider.Domain.Enums;
using OtpProvider.Domain.Security;
using OtpProvider.Infrastructure.Factory;
using System.Security.Cryptography;

namespace WebApi.Practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OtpController : ControllerBase
    {
        private readonly OtpSenderFactory _factory;
        private readonly EmailServiceFactory _emailFactory;
        private readonly IUnitOfWork _uow;
        private const int OtpExpirySeconds = 300; // 5 minutes
        private const int MaxVerificationAttempts = 3;

        public OtpController(OtpSenderFactory factory, EmailServiceFactory emailFactory, IUnitOfWork uow)
        {
            _factory = factory;
            _emailFactory = emailFactory;
            _uow = uow;
        }

        [HttpGet]
        public IActionResult Get() => Ok("Request received.");

        [HttpPost("send")]
        // [Authorize(Roles = "User")]
        [ProducesResponseType(typeof(SendOtpResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<SendOtpResponse>> Send([FromBody] SendOtpRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(new SendOtpResponse { IsSent = false, ErrorMessage = "Invalid request." });

            var rawOtp = RandomNumberGenerator.GetInt32(0, 10000).ToString("D4");
            var hashedOtp = SecurityHashing.HashOtp(rawOtp);

            // find provider
            var provider = (await _uow.OtpProviderRepository.GetAllAsync(onlyActive: true, ct))
                .FirstOrDefault(p => p.DeliveryType == request.Method);

            if (provider is null)
            {
                return BadRequest(new SendOtpResponse { IsSent = false, ErrorMessage = "No active OTP provider available for the specified method." });
            }

            var now = DateTime.UtcNow;
            var otpRequest = new OtpRequest
            {
                RequestId = Guid.NewGuid(),
                SentTo = request.To,
                OtpMethod = request.Method,
                OtpHashed = hashedOtp,
                CreatedAt = now,
                ExpiresAt = now.AddSeconds(OtpExpirySeconds),
                IsUsed = false,
                SendStatus = OtpSendStatus.Pending,
                OtpProviderId = provider.Id,
                AttemptCount = 0,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                DeviceInfo = Request.Headers.UserAgent.ToString(),
                ErrorMessage = string.Empty
            };

            await _uow.OtpRepository.AddOtpRequestAsync(otpRequest, ct);
            await _uow.SaveChangesAsync(ct);

            bool sendSucceeded = false;
            string? sendError = null;

            try
            {
                var sender = _factory.GetSender(request.Method);
                sendSucceeded = await sender.SendOtp(request.To, rawOtp);
                otpRequest.SendStatus = sendSucceeded ? OtpSendStatus.Success : OtpSendStatus.Failed;
                if (!sendSucceeded)
                {
                    sendError = "Provider reported failure.";
                    otpRequest.ErrorMessage = sendError;
                }
            }
            catch (Exception ex)
            {
                sendError = ex.Message;
                otpRequest.SendStatus = OtpSendStatus.Failed;
                otpRequest.ErrorMessage = ex.Message;
            }

            await _uow.OtpRepository.UpdateOtpRequestAsync(otpRequest, ct);
            await _uow.SaveChangesAsync(ct);

            return Ok(new SendOtpResponse
            {
                RequestId = otpRequest.RequestId,
                OtpExpirySeconds = OtpExpirySeconds,
                IsSent = sendSucceeded,
                ErrorMessage = sendError
            });
        }

        [HttpPost("verify")]
        //[Authorize(Roles = "User")]
        [ProducesResponseType(typeof(OtpVerifyResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<OtpVerifyResponse>> Verify([FromBody] OtpVerifyRequest request, CancellationToken ct)
        {
            if (request.RequestId == Guid.Empty || string.IsNullOrWhiteSpace(request.Otp))
                return BadRequest(new OtpVerifyResponse { IsSuccessful = false, ErrorMessage = "RequestId and Otp are required." });

            var otpRequest = await _uow.OtpRepository.GetByRequestIdAsync(request.RequestId, ct);
            if (otpRequest is null)
                return NotFound(new OtpVerifyResponse { IsSuccessful = false, ErrorMessage = "OTP request not found." });

            var now = DateTime.UtcNow;
            var verification = new OtpVerification
            {
                OtpRequestId = otpRequest.Id,
                ProvidedOtpHashed = SecurityHashing.HashOtp(request.Otp),
                AttemptedAtUtc = now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                DeviceInfo = Request.Headers.UserAgent.ToString()
            };

            if (otpRequest.SendStatus != OtpSendStatus.Success)
            {
                verification.IsSuccessful = false;
                verification.FailureReason = OtpFailureReason.ProviderError;
                otpRequest.AttemptCount++;
                await _uow.OtpRepository.AddVerificationAsync(verification, ct);
                await _uow.SaveChangesAsync(ct);
                return Ok(new OtpVerifyResponse { IsSuccessful = false, ErrorMessage = "OTP not deliverable." });
            }

            if (otpRequest.IsUsed)
            {
                verification.IsSuccessful = false;
                verification.FailureReason = OtpFailureReason.AlreadyUsed;
                otpRequest.AttemptCount++;
                await _uow.OtpRepository.AddVerificationAsync(verification, ct);
                await _uow.SaveChangesAsync(ct);
                return Ok(new OtpVerifyResponse { IsSuccessful = false, ErrorMessage = "OTP already used." });
            }

            if (now > otpRequest.ExpiresAt)
            {
                verification.IsSuccessful = false;
                verification.FailureReason = OtpFailureReason.Expired;
                otpRequest.AttemptCount++;
                await _uow.OtpRepository.AddVerificationAsync(verification, ct);
                await _uow.SaveChangesAsync(ct);
                return Ok(new OtpVerifyResponse { IsSuccessful = false, ErrorMessage = "OTP expired." });
            }

            if (otpRequest.AttemptCount >= MaxVerificationAttempts)
            {
                verification.IsSuccessful = false;
                verification.FailureReason = OtpFailureReason.LockedOut;
                await _uow.OtpRepository.AddVerificationAsync(verification, ct);
                await _uow.SaveChangesAsync(ct);
                return Ok(new OtpVerifyResponse { IsSuccessful = false, ErrorMessage = "Maximum attempts exceeded." });
            }

            if (!SecurityHashing.VerifyOtp(request.Otp, otpRequest.OtpHashed))
            {
                verification.IsSuccessful = false;
                verification.FailureReason = OtpFailureReason.InvalidOtp;
                otpRequest.AttemptCount++;
                await _uow.OtpRepository.AddVerificationAsync(verification, ct);
                await _uow.SaveChangesAsync(ct);
                return Ok(new OtpVerifyResponse { IsSuccessful = false, ErrorMessage = "Invalid OTP." });
            }

            verification.IsSuccessful = true;
            otpRequest.IsUsed = true;
            otpRequest.VerifiedAt = now;
            otpRequest.AttemptCount++;
            await _uow.OtpRepository.AddVerificationAsync(verification, ct);
            await _uow.OtpRepository.UpdateOtpRequestAsync(otpRequest, ct);
            await _uow.SaveChangesAsync(ct);

            return Ok(new OtpVerifyResponse { IsSuccessful = true });
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Admin")]
        public IActionResult SendBulkEmail([FromBody] BulkEmailRequest request)
        {
            var emailService = _emailFactory.GetEmailService(request.Provider);
            if (emailService is IBulkEmailService bulkSender)
            {
                bulkSender.SendBulkEmail(request.ToList, request.Subject, request.Body);
                return Ok("Bulk email sent");
            }
            return BadRequest("Bulk email not supported by the current provider.");
        }
    }
}
