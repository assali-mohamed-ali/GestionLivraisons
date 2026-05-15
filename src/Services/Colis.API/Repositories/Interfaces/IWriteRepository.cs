namespace Colis.API.Repositories.Interfaces;

// ISP: Write clients depend on IWriteRepository<T>
public interface IWriteRepository<T> where T : class
{
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}
