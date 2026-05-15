using Shared.Contracts.DTOs;

namespace Livreur.API.Services.Interfaces;

public interface ILivreurReadService
{
    Task<IReadOnlyList<LivreurDto>> GetAllAsync();
    Task<LivreurDto?> GetByIdAsync(int id);
}

public interface ILivreurAdminService : ILivreurReadService
{
    Task<LivreurDto> CreateAsync(CreateLivreurDto dto);
    Task UpdateAsync(int id, CreateLivreurDto dto);
    Task DeleteAsync(int id);
    Task<LivreurStatsDto> GetStatsAsync();
}
