using Microsoft.EntityFrameworkCore;
using OtpProvider.Domain.Entities;
using System.Threading.Tasks;

namespace OtpProvider.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<ApplicationUser> Users { get; }
        DbSet<ApplicationRole> Roles { get; }
        DbSet<ApplicationUserRoles> UserRoles { get; }

        DbSet<OtpProvider.Domain.Entities.OtpProvider> OtpProviders { get; }
        DbSet<OtpRequest> OtpRequests { get; }
        DbSet<OtpVerification> OtpVerifications { get; }

        Task<int> SaveChangesAsync();
    }
}
