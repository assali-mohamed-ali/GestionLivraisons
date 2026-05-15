using Livreur.API.Data;
using Livreur.API.Models;
using Livreur.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Livreur.API.Repositories;

public class VehiculeRepository(LivreurDbContext context) : Repository<Vehicule>(context), IVehiculeRepository
{
    private readonly LivreurDbContext _livreurContext = context;

    public async Task<bool> MatriculeExistsAsync(string matricule, int? excludeId = null) =>
        await _livreurContext.Vehicules
            .AnyAsync(v => v.Matricule == matricule && (!excludeId.HasValue || v.Id != excludeId));

    public async Task<IReadOnlyList<Vehicule>> GetByLivreurAsync(int livreurId) =>
        await _livreurContext.Vehicules.Where(v => v.LivreurId == livreurId).ToListAsync();
}
