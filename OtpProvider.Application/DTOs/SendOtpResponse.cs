namespace OtpProvider.Application.DTOs
{
    public class SendOtpResponse
    {
        public Guid? RequestId { get; set; }
        public int OtpExpirySeconds { get; set; }
        public bool IsSent { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
