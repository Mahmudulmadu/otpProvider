namespace OtpProvider.Application.Interfaces

{
    public interface IBulkEmailService : IEmailService
    {
        void SendBulkEmail(List<string> recipients, string subject, string body);
    }
}
