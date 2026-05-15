using Livreur.API.Repositories.Interfaces;

namespace Livreur.API.UnitOfWork;

public interface ILivreurUnitOfWork : IDisposable, IAsyncDisposable
{
    ILivreurRepository Livreurs { get; }
    IVehiculeRepository Vehicules { get; }
    Task<int> SaveAsync();
}
