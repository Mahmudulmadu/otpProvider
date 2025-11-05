using Microsoft.EntityFrameworkCore.Storage;
using OtpProvider.Application.Interfaces;
using OtpProvider.Application.Interfaces.IRepository;
using OtpProvider.Domain.Interfaces;
using OtpProvider.Infrastructure.Repositories;


namespace OtpProvider.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        private OtpRepository? _otpRepository;
       // private UserRepository? _userRepository;
        private OtpProviderRepository? _otpProviderRepository;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
        }

        public IOtpRepository OtpRepository => _otpRepository ??= new OtpRepository(_db);
        //public IUserRepository UserRepository => _userRepository ??= new UserRepository(_db);
        public IOtpProviderRepository OtpProviderRepository => _otpProviderRepository ??= new OtpProviderRepository(_db);

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _db.SaveChangesAsync(ct);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        {
            return await _db.Database.BeginTransactionAsync(ct);
        }

        public async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken ct = default)
        {
            await transaction.CommitAsync(ct);
            await transaction.DisposeAsync();
        }

        public async Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken ct = default)
        {
            await transaction.RollbackAsync(ct);
            await transaction.DisposeAsync();
        }

        public void Dispose() => _db.Dispose();
    }
}
