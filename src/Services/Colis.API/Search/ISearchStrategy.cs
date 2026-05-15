using Shared.Contracts.DTOs;

namespace Colis.API.Search;

// OCP hook point: add new search filters by implementing this interface
public interface ISearchStrategy<T>
{
    IQueryable<T> Apply(IQueryable<T> query, ColisSearchParams p);
}
