namespace OtpProvider.Application.DTOs
{
    public class OtpVerifyRequest
    {
        public Guid RequestId { get; set; }
        public string Otp { get; set; } = string.Empty;
    }
}
