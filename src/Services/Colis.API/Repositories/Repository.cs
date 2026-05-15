using Colis.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Colis.API.Repositories;

// DIP: depends on DbContext abstraction via EF
// SRP: only data access — no business logic, no validation
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _set;

    public Repository(DbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) =>
        await _set.FindAsync(id);

    public async Task<IReadOnlyList<T>> GetAllAsync() =>
        await _set.ToListAsync();

    public async Task<bool> ExistsAsync(int id) =>
        await _set.FindAsync(id) is not null;

    public async Task AddAsync(T entity) =>
        await _set.AddAsync(entity);

    public void Update(T entity) =>
        _context.Entry(entity).State = EntityState.Modified;

    public void Delete(T entity) =>
        _set.Remove(entity);
}
