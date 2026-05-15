using Colis.API.Repositories.Interfaces;

namespace Colis.API.UnitOfWork;

// IDisposable + ISP interface for Unit of Work
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    // Each repo exposed as its domain-specific interface (ISP)
    IColisRepository Colis { get; }
    Task<int> SaveAsync();
}
