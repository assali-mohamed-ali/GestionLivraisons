using Shared.Contracts.DTOs;

namespace Colis.API.Services.Interfaces;

// ISP: Admin needs full service
public interface IColisAdminService : IColisReadService
{
    Task<ColisDto> CreateAsync(CreateColisDto dto);
    Task UpdateAsync(int id, CreateColisDto dto);
    Task DeleteAsync(int id);
    Task<ColisStatsDto> GetStatsAsync();
}
