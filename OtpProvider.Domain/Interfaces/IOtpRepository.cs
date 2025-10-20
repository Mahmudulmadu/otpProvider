using OtpProvider.Domain.Entities;


namespace OtpProvider.Domain.Interfaces
{
    public interface IOtpRepository
    {
        Task AddOtpRequestAsync(OtpRequest request, CancellationToken ct = default);
        Task<OtpRequest?> GetByRequestIdAsync(Guid requestId, CancellationToken ct = default);
        Task UpdateOtpRequestAsync(OtpRequest request, CancellationToken ct = default);
        Task AddVerificationAsync(OtpVerification verification, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}