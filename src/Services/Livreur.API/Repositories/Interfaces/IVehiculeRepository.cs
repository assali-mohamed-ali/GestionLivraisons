using Livreur.API.Models;

namespace Livreur.API.Repositories.Interfaces;

public interface IVehiculeRepository : IRepository<Vehicule>
{
    Task<bool> MatriculeExistsAsync(string matricule, int? excludeId = null);
    Task<IReadOnlyList<Vehicule>> GetByLivreurAsync(int livreurId);
}
