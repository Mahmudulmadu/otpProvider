
using OtpProvider.Domain.Enums;
using System.ComponentModel.DataAnnotations;

 namespace OtpProvider.Application.DTOs
{
    public class SendOtpRequest
    {
        [Required]
        public OtpMethod Method { get; set; }

        [Required]
        public required string To { get; set; }

        public string Purpose { get; set; } = string.Empty;
    }
}
