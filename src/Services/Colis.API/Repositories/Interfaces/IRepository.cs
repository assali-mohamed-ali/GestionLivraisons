namespace Colis.API.Repositories.Interfaces;

// ISP: Full repo composes both Read + Write — only used where CRUD is needed
public interface IRepository<T> : IReadRepository<T>, IWriteRepository<T> where T : class { }
