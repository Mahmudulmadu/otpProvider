using Microsoft.Extensions.DependencyInjection;
using OtpProvider.Domain.Enums;
using OtpProvider.Domain.Interfaces;

using OtpProvider.Infrastructure.OtpSenders;

namespace OtpProvider.Infrastructure.Factory
{
    public class OtpSenderFactory
    {
        private readonly IServiceProvider _provider;
        private readonly EmailServiceFactory _emailFactory;

        public OtpSenderFactory(IServiceProvider provider, EmailServiceFactory emailFactory)
        {
            _provider = provider;
            _emailFactory = emailFactory;
        }

        public OtpProvider.Domain.Interfaces.IOtpSender GetSender(OtpMethod method)
        {
            return method switch
            {
                OtpMethod.SMS => _provider.GetRequiredService<SmsOtpSender>(),
                OtpMethod.Email => new EmailOtpSender(_emailFactory.GetDefaultEmailService()),
                OtpMethod.WhatsApp => _provider.GetRequiredService<WhatsAppOtpSender>(),
                _ => throw new Exception("Invalid OTP method")
            };
        }
    }
}
