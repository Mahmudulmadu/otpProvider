using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OtpProvider.Domain.Entities;

namespace OtpProvider.Application.Interfaces.IRepository
{
    public interface IOtpProviderRepository
    {
        IQueryable<OtpProviderEntity> Query();
        Task<List<OtpProviderEntity>> GetAllAsync(bool onlyActive = false, CancellationToken ct = default);
        Task<OtpProviderEntity?> GetByIdAsync(int id, CancellationToken ct = default);
        Task AddAsync(OtpProviderEntity entity, CancellationToken ct = default);
        void Update(OtpProviderEntity entity);
        void Remove(OtpProviderEntity entity);
    }
}
