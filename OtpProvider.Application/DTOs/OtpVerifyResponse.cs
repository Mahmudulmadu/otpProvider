namespace OtpProvider.Application.DTOs
{
    public class OtpVerifyResponse
    {
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
