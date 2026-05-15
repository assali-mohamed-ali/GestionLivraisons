using Shared.Contracts.DTOs;

namespace Colis.API.Search;

// OCP: Engine composes all strategies — closed for modification
// Adding a new search filter only requires a new ISearchStrategy implementation
public class ColisSearchEngine(IEnumerable<ISearchStrategy<Models.Colis>> strategies)
{
    public IQueryable<Models.Colis> Apply(IQueryable<Models.Colis> query, ColisSearchParams p)
    {
        foreach (var strategy in strategies) query = strategy.Apply(query, p);
        return query;
    }
}
