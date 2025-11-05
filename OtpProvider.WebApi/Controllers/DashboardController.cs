using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtpProvider.Application.DTOs;
using OtpProvider.Application.Interfaces;
using OtpProvider.Application.Interfaces.IRepository;
using OtpProvider.Domain.Enums;
using OtpProvider.Domain.Entities;
namespace OtpProvider.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IUnitOfWork _uow;

        public DashboardController(IUnitOfWork uow) => _uow = uow;

        [HttpGet("stats")]
        public async Task<ActionResult<OtpStatsResponse>> GetStats([FromQuery] int minutes = 1440, CancellationToken ct = default)
        {
            if (minutes <= 0) minutes = 60;
            if (minutes > 60 * 24 * 30) minutes = 60 * 24 * 30;

            var fromUtc = DateTime.UtcNow.AddMinutes(-minutes);

            var query = _uow.OtpRepository
                .Query() // we didn't create Query() in IOtpRepository earlier; assume it exists
                .AsNoTracking()
                .Where(r => r.CreatedAt >= fromUtc);

            var aggregated = await query
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalSent = g.Count(),
                    TotalVerified = g.Count(r => r.IsUsed),
                    TotalFailed = g.Count(r => r.SendStatus == OtpSendStatus.Failed),
                    TotalPending = g.Count(r => r.SendStatus == OtpSendStatus.Pending)
                })
                .FirstOrDefaultAsync(ct);

            var totalSent = aggregated?.TotalSent ?? 0;
            var totalVerified = aggregated?.TotalVerified ?? 0;
            var totalFailed = aggregated?.TotalFailed ?? 0;
            var totalPending = aggregated?.TotalPending ?? 0;

            double successRate = totalSent == 0 ? 0d : Math.Round((double)totalVerified / totalSent, 4);

            return Ok(new OtpStatsResponse(
                RangeMinutes: minutes,
                TotalSent: totalSent,
                TotalVerified: totalVerified,
                TotalFailed: totalFailed,
                TotalPending: totalPending,
                SuccessRate: successRate,
                GeneratedAtUtc: DateTime.UtcNow.ToString("O")
            ));
        }

        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<OtpRecentItemDto>>> GetRecent([FromQuery] int limit = 10, CancellationToken ct = default)
        {
            limit = Math.Clamp(limit, 1, 100);

            var list = await _uow.OtpRepository
                .Query()
                .AsNoTracking()
                .Include(r => r.OtpProvider)
                .OrderByDescending(r => r.CreatedAt)
                .Take(limit)
                .Select(r => new OtpRecentItemDto
                {
                    Id = r.RequestId,
                    Destination = r.SentTo,
                    Provider = r.OtpProvider.Name,
                    CreatedUtc = r.CreatedAt,
                    Status = r.IsUsed ? "Verified" : r.SendStatus.ToString()
                })
                .ToListAsync(ct);

            return Ok(list);
        }
    }
}
