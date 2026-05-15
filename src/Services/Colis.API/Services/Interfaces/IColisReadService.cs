using Shared.Contracts.DTOs;

namespace Colis.API.Services.Interfaces;

// ISP: User role only needs read service
public interface IColisReadService
{
    Task<PagedResult<ColisDto>> SearchAsync(ColisSearchParams p, int page = 1, int pageSize = 10);
    Task<ColisDto?> GetByIdAsync(int id);
}
