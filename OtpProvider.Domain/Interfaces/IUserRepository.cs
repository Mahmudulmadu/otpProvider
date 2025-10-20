using OtpProvider.Domain.Entities;


namespace OtpProvider.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByUserNameAsync(string username, CancellationToken ct = default);
        Task<bool> ExistsByUserNameAsync(string username, CancellationToken ct = default);
        Task AddAsync(ApplicationUser user, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}