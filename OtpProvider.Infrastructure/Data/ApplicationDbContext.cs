using Microsoft.EntityFrameworkCore;
using OtpProvider.Application.Interfaces;
using OtpProvider.Domain.Entities;
using System.Threading.Tasks;

namespace OtpProvider.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<ApplicationRole> Roles { get; set; }
        public DbSet<ApplicationUserRoles> UserRoles { get; set; }

        public DbSet<OtpProvider.Domain.Entities.OtpProviderEntity> OtpProviders { get; set; }
        public DbSet<OtpRequest> OtpRequests { get; set; }
        public DbSet<OtpVerification> OtpVerifications { get; set; }

        public async Task<int> SaveChangesAsync() => await base.SaveChangesAsync();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ApplicationUser
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(u => u.UserName).IsUnique();
                entity.Property(u => u.UserName).IsRequired().HasMaxLength(25);
                entity.Property(u => u.Email).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();
            });

            // Roles
            modelBuilder.Entity<ApplicationRole>(entity =>
            {
                entity.HasIndex(r => r.Name).IsUnique();
                entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
            });

            // UserRoles
            modelBuilder.Entity<ApplicationUserRoles>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
                entity.HasOne(ur => ur.User).WithMany(u => u.UserRoles)
                      .HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ur => ur.Role).WithMany(r => r.UserRoles)
                      .HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Cascade);
            });

            // OtpProvider
            modelBuilder.Entity<OtpProvider.Domain.Entities.OtpProviderEntity>(entity =>
            {
                entity.HasIndex(p => p.Name).IsUnique();
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Description).HasMaxLength(512);
                entity.Property(p => p.ConfigurationJson).HasMaxLength(2048);
            });

            // OtpRequest
            modelBuilder.Entity<OtpRequest>(entity =>
            {
                entity.HasIndex(r => r.RequestId).IsUnique();
                entity.HasIndex(r => new { r.RequestId, r.OtpHashed });

                entity.Property(r => r.SentTo).IsRequired().HasMaxLength(256);
                entity.Property(r => r.OtpHashed).IsRequired().HasMaxLength(256);

                entity.HasOne(r => r.OtpProvider).WithMany(p => p.OtpRequests)
                      .HasForeignKey(r => r.OtpProviderId).OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.SendByUser).WithMany()
                      .HasForeignKey(r => r.SendByUserId).OnDelete(DeleteBehavior.SetNull);
            });

            // OtpVerification
            modelBuilder.Entity<OtpVerification>(entity =>
            {
                entity.Property(v => v.ProvidedOtpHashed).IsRequired().HasMaxLength(256);
                entity.HasOne(v => v.OtpRequest).WithMany(r => r.OtpVerifications)
                      .HasForeignKey(v => v.OtpRequestId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
