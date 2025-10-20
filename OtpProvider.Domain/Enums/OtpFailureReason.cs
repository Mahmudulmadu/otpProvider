namespace OtpProvider.Domain.Enums
{
    public enum OtpFailureReason
    {
        Expired,
        AlreadyUsed,
        InvalidOtp,
        ProviderError,
        LockedOut,
        Unknown
    }
}
