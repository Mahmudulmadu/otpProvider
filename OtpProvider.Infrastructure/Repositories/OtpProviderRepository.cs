using Microsoft.EntityFrameworkCore;
using OtpProvider.Application.Interfaces;
using OtpProvider.Application.Interfaces.IRepository;
using OtpProvider.Domain.Entities;
using OtpProvider.Infrastructure.Data;

namespace OtpProvider.Infrastructure.Repositories
{
    public class OtpProviderRepository : IOtpProviderRepository
    {
        private readonly ApplicationDbContext _db;
        public OtpProviderRepository(ApplicationDbContext db) => _db = db;

        public IQueryable<OtpProviderEntity> Query() => _db.OtpProviders.AsQueryable();

        public async Task<List<OtpProviderEntity>> GetAllAsync(bool onlyActive = false, CancellationToken ct = default)
        {
            var q = _db.OtpProviders.AsNoTracking();
            if (onlyActive) q = q.Where(p => p.IsActive);
            return await q.OrderBy(p => p.Name).ToListAsync(ct);
        }

        public async Task<OtpProviderEntity?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _db.OtpProviders.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task AddAsync(OtpProviderEntity entity, CancellationToken ct = default)
        {
            await _db.OtpProviders.AddAsync(entity, ct);
        }

        public void Update(OtpProviderEntity entity) => _db.OtpProviders.Update(entity);

        public void Remove(OtpProviderEntity entity) => _db.OtpProviders.Remove(entity);
    }
}
