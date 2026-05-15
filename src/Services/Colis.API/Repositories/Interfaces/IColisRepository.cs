using Colis.API.Models;
using Shared.Contracts.DTOs;

namespace Colis.API.Repositories.Interfaces;

// Domain-specific full interface — admin needs this
public interface IColisRepository : IRepository<Models.Colis>
{
    Task<(IReadOnlyList<Models.Colis> Items, int Total)> SearchPagedAsync(
        ColisSearchParams p, int page, int pageSize);
    Task<IReadOnlyList<Models.Colis>> GetByLivreurAsync(int livreurId);
    Task<IReadOnlyList<Models.Colis>> GetRecentAsync(int count);
    Task<ColisStatsProjection> GetStatsAsync();
}
