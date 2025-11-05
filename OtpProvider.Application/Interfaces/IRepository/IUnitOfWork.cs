using Microsoft.EntityFrameworkCore.Storage;
using OtpProvider.Domain.Interfaces;


namespace OtpProvider.Application.Interfaces.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IOtpRepository OtpRepository { get; }
        //IUserRepository UserRepository { get; }
        IOtpProviderRepository OtpProviderRepository { get; }
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
        Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken ct = default);
        Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken ct = default);
    }
}