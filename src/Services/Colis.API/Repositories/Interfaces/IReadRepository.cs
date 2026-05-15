namespace Colis.API.Repositories.Interfaces;

// ISP: Read-only clients only depend on IReadRepository<T>
public interface IReadRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<bool> ExistsAsync(int id);
}
