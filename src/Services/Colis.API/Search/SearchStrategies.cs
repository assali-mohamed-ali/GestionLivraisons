using Shared.Contracts.DTOs;

namespace Colis.API.Search;

public class LibelleSearchStrategy : ISearchStrategy<Models.Colis>
{
    public IQueryable<Models.Colis> Apply(IQueryable<Models.Colis> query, ColisSearchParams p) =>
        string.IsNullOrWhiteSpace(p.Libelle) ? query : query.Where(c => c.Libelle.Contains(p.Libelle));
}

public class MontantRangeStrategy : ISearchStrategy<Models.Colis>
{
    public IQueryable<Models.Colis> Apply(IQueryable<Models.Colis> query, ColisSearchParams p)
    {
        if (p.MinMontant.HasValue) query = query.Where(c => c.Montant >= p.MinMontant);
        if (p.MaxMontant.HasValue) query = query.Where(c => c.Montant <= p.MaxMontant);
        return query;
    }
}

public class PoidsMaxStrategy : ISearchStrategy<Models.Colis>
{
    public IQueryable<Models.Colis> Apply(IQueryable<Models.Colis> query, ColisSearchParams p) =>
        p.MaxPoids.HasValue ? query.Where(c => c.Poids <= p.MaxPoids) : query;
}

public class DateLivraisonStrategy : ISearchStrategy<Models.Colis>
{
    public IQueryable<Models.Colis> Apply(IQueryable<Models.Colis> query, ColisSearchParams p) =>
        p.DateLivraison.HasValue ? query.Where(c => c.DateLivraison.Date == p.DateLivraison.Value.Date) : query;
}

public class ClientIdStrategy : ISearchStrategy<Models.Colis>
{
    public IQueryable<Models.Colis> Apply(IQueryable<Models.Colis> query, ColisSearchParams p) =>
        p.ClientId.HasValue ? query.Where(c => c.ClientId == p.ClientId.Value) : query;
}
