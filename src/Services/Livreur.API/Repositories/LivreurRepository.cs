using Livreur.API.Data;
using Livreur.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Livreur.API.Repositories;

public class LivreurRepository(LivreurDbContext context) : Repository<Models.Livreur>(context), ILivreurRepository
{
    private readonly LivreurDbContext _livreurContext = context;

    public async Task<bool> CINExistsAsync(string cin, int? excludeId = null) =>
        await _livreurContext.Livreurs
            .AnyAsync(l => l.CIN == cin && (!excludeId.HasValue || l.Id != excludeId));

    public async Task<IReadOnlyList<Models.Livreur>> GetByVilleAsync(string ville) =>
        await _livreurContext.Livreurs.Where(l => l.Ville == ville).ToListAsync();
}
