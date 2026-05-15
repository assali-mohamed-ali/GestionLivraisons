namespace Livreur.API.Repositories.Interfaces;

public interface IReadRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<bool> ExistsAsync(int id);
}

public interface IWriteRepository<T> where T : class
{
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public interface IRepository<T> : IReadRepository<T>, IWriteRepository<T> where T : class { }
