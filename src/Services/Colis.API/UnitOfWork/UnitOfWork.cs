using Colis.API.Data;
using Colis.API.Repositories;
using Colis.API.Repositories.Interfaces;
using Colis.API.Search;

namespace Colis.API.UnitOfWork;

// IDisposable full canonical pattern — PATTERN 1 + PATTERN 4
public class UnitOfWork : IUnitOfWork
{
    private readonly ColisDbContext _context;

    // Lazy repo initialization — only created when accessed (avoids unnecessary allocations)
    private IColisRepository? _colisRepo;

    private bool _disposed = false;

    public UnitOfWork(ColisDbContext context, ColisSearchEngine searchEngine)
    {
        _context      = context;
        _searchEngine = searchEngine;
    }

    private readonly ColisSearchEngine _searchEngine;

    // Lazy property — SRP: UoW doesn't create repos eagerly; it coordinates them
    public IColisRepository Colis =>
        _colisRepo ??= new ColisRepository(_context, _searchEngine);

    public async Task<int> SaveAsync()
    {
        // Could add: domain event dispatching, audit logging here (single place)
        return await _context.SaveChangesAsync();
    }

    // IDisposable — canonical full pattern
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this); // Prevent finalizer from running — we already cleaned up
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
                _context.Dispose(); // Dispose managed resources
            // Unmanaged resources would be freed here regardless of 'disposing'
            _disposed = true;
        }
    }

    // Finalizer — safety net for consumers who forget to dispose
    ~UnitOfWork() => Dispose(disposing: false);

    // IAsyncDisposable — prefer this in async contexts
    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
