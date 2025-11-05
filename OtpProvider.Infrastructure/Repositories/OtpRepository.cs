using Microsoft.EntityFrameworkCore;
using OtpProvider.Application.Interfaces;
using OtpProvider.Domain.Entities;
using OtpProvider.Domain.Interfaces;
using OtpProvider.Infrastructure.Data;


namespace OtpProvider.Infrastructure.Repositories
{
    public class OtpRepository : IOtpRepository
    {
        private readonly ApplicationDbContext _db;


        public OtpRepository(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task AddOtpRequestAsync(OtpRequest request, CancellationToken ct = default)
        {
            await _db.OtpRequests.AddAsync(request, ct);
        }
        public IQueryable<OtpRequest> Query() => _db.OtpRequests.AsQueryable();

        public async Task<OtpRequest?> GetByRequestIdAsync(Guid requestId, CancellationToken ct = default)
        {
            return await _db.OtpRequests
            .Include(r => r.OtpVerifications)
            .Include(r => r.OtpProvider)
            .FirstOrDefaultAsync(r => r.RequestId == requestId, ct);
        }


        public Task UpdateOtpRequestAsync(OtpRequest request, CancellationToken ct = default)
        {
            _db.OtpRequests.Update(request);
            return Task.CompletedTask;
        }


        public async Task AddVerificationAsync(OtpVerification verification, CancellationToken ct = default)
        {
            await _db.OtpVerifications.AddAsync(verification, ct);
        }


        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}