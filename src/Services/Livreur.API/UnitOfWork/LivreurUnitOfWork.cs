using Livreur.API.Data;
using Livreur.API.Repositories;
using Livreur.API.Repositories.Interfaces;

namespace Livreur.API.UnitOfWork;

// Full IDisposable pattern — PATTERN 1 + PATTERN 4
public class LivreurUnitOfWork : ILivreurUnitOfWork
{
    private readonly LivreurDbContext _context;
    private ILivreurRepository?  _livreurRepo;
    private IVehiculeRepository? _vehiculeRepo;
    private bool _disposed = false;

    public LivreurUnitOfWork(LivreurDbContext context) => _context = context;

    public ILivreurRepository  Livreurs  => _livreurRepo  ??= new LivreurRepository(_context);
    public IVehiculeRepository Vehicules => _vehiculeRepo ??= new VehiculeRepository(_context);

    public async Task<int> SaveAsync() => await _context.SaveChangesAsync();

    // IDisposable — canonical full pattern
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
                _context.Dispose(); // Dispose managed resources
            _disposed = true;
        }
    }

    // Finalizer — safety net
    ~LivreurUnitOfWork() => Dispose(disposing: false);

    // IAsyncDisposable
    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
