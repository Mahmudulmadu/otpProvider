using OtpProvider.Application.DTOs;
namespace OtpProvider.Application.Mapping
{
    public static class OtpProviderMappingExtensions
    {
        public static OtpProviderDto ToDto(this OtpProvider.Domain.Entities.OtpProvider entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            DeliveryType = entity.DeliveryType,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            ConfigurationJson = entity.ConfigurationJson
        };

        public static void ApplyUpdate(this OtpProvider.Domain.Entities.OtpProvider entity, OtpProviderUpdateDto dto)
        {
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.DeliveryType = dto.DeliveryType;
            entity.IsActive = dto.IsActive;
            entity.ConfigurationJson = dto.ConfigurationJson;
        }
    }
}