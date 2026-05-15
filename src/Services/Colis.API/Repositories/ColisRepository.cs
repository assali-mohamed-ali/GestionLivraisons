using Colis.API.Data;
using Colis.API.Models;
using Colis.API.Repositories.Interfaces;
using Colis.API.Search;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.DTOs;

namespace Colis.API.Repositories;

// SRP: only Colis data access logic — no business rules, no HTTP
public class ColisRepository(ColisDbContext context, ColisSearchEngine searchEngine)
    : Repository<Models.Colis>(context), IColisRepository
{
    private readonly ColisDbContext _colisContext = context;

    public async Task<(IReadOnlyList<Models.Colis> Items, int Total)> SearchPagedAsync(
        ColisSearchParams p, int page, int pageSize)
    {
        // OCP: delegates to search engine which composes strategy objects
        var query = searchEngine.Apply(_colisContext.Colis.AsQueryable(), p);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.DateLivraison)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<IReadOnlyList<Models.Colis>> GetByLivreurAsync(int livreurId) =>
        await _colisContext.Colis.Where(c => c.LivreurId == livreurId).ToListAsync();

    public async Task<IReadOnlyList<Models.Colis>> GetRecentAsync(int count) =>
        await _colisContext.Colis
            .OrderByDescending(c => c.DateLivraison)
            .Take(count)
            .ToListAsync();

    public async Task<ColisStatsProjection> GetStatsAsync()
    {
        var all = await _colisContext.Colis.ToListAsync();
        return new ColisStatsProjection
        {
            TotalColis    = all.Count,
            TotalMontant  = all.Sum(c => c.Montant),
            ColisParLivreur = all
                .GroupBy(c => c.LivreurId)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            MontantParMois  = all
                .GroupBy(c => c.DateLivraison.ToString("yyyy-MM"))
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Montant))
        };
    }
}
