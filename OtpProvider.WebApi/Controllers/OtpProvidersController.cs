using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtpProvider.Application.DTOs;
using OtpProvider.Application.Interfaces.IRepository;
using OtpProvider.Application.Mapping;
using OtpProvider.Domain;
using OtpProvider.Domain.Enums;
using OtpProvider.Infrastructure.Data;
using OtpProvider.Application.Interfaces;
using OtpProvider.Domain.Entities;

namespace WebApi.Practice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OtpProvidersController : ControllerBase
    {
        private readonly IUnitOfWork _uow;

        public OtpProvidersController(IUnitOfWork uow) => _uow = uow;

        public record DeliveryTypeOption(string Value, string Label);

        [HttpGet("delivery-types")]
        public ActionResult<IEnumerable<DeliveryTypeOption>> GetDeliveryTypes()
        {
            var values = Enum.GetNames(typeof(OtpProvider.Domain.Enums.OtpMethod))
                .Select(v => new DeliveryTypeOption(v, v))
                .ToList();
            return Ok(values);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OtpProviderDto>>> GetAll([FromQuery] bool onlyActive = false, CancellationToken ct = default)
        {
            var list = await _uow.OtpProviderRepository.GetAllAsync(onlyActive, ct);
            return Ok(list.Select(p => p.ToDto()));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OtpProviderDto>> GetById(int id, CancellationToken ct = default)
        {
            var entity = await _uow.OtpProviderRepository.GetByIdAsync(id, ct);
            if (entity is null) return NotFound();
            return Ok(entity.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<OtpProviderDto>> Create([FromBody] OtpProviderCreateDto dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            bool nameExists = (await _uow.OtpProviderRepository.GetAllAsync(false, ct)).Any(p => p.Name == dto.Name);
            if (nameExists) return Conflict($"An OTP provider with name '{dto.Name}' already exists.");

            var entity = new OtpProvider.Domain.Entities.OtpProviderEntity
            {
                Name = dto.Name,
                Description = dto.Description,
                DeliveryType = dto.DeliveryType,
                IsActive = dto.IsActive,
                ConfigurationJson = dto.ConfigurationJson,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.OtpProviderRepository.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<OtpProviderDto>> Update(int id, [FromBody] OtpProviderUpdateDto dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var entity = await _uow.OtpProviderRepository.GetByIdAsync(id, ct);
            if (entity is null) return NotFound();

            bool duplicateName = (await _uow.OtpProviderRepository.GetAllAsync(false, ct))
                .Any(p => p.Id != id && p.Name == dto.Name);

            if (duplicateName) return Conflict($"Another OTP provider already uses name '{dto.Name}'.");

            entity.ApplyUpdate(dto);
            _uow.OtpProviderRepository.Update(entity);
            await _uow.SaveChangesAsync(ct);

            return Ok(entity.ToDto());
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] bool hard = false, CancellationToken ct = default)
        {
            var entity = await _uow.OtpProviderRepository.GetByIdAsync(id, ct);
            if (entity is null) return NotFound();

            if (hard)
            {
                _uow.OtpProviderRepository.Remove(entity);
            }
            else
            {
                if (!entity.IsActive) return NoContent();
                entity.IsActive = false;
                _uow.OtpProviderRepository.Update(entity);
            }

            await _uow.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}