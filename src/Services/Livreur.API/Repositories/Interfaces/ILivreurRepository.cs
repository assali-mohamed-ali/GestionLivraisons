namespace Livreur.API.Repositories.Interfaces;

public interface ILivreurRepository : IRepository<Models.Livreur>
{
    Task<bool> CINExistsAsync(string cin, int? excludeId = null);
    Task<IReadOnlyList<Models.Livreur>> GetByVilleAsync(string ville);
}
