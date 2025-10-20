using OtpProvider.Domain;
using OtpProvider.Domain.Enums;


namespace OtpProvider.Domain.Interfaces
{
    public interface IOtpSenderFactory
    {
        IOtpSender GetSender(OtpMethod method);
    }
}