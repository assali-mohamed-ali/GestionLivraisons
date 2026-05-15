namespace Shared.Contracts.DTOs;

public record ColisDto(
    int Id, string Libelle, DateTime DateLivraison,
    decimal Montant, double Poids, double Volume,
    int LivreurId, string? LivreurNom, int ClientId, string? ClientNom);

public record CreateColisDto(
    string Libelle, DateTime DateLivraison,
    decimal Montant, double Poids, double Volume,
    int LivreurId, int ClientId);

public record ColisSearchParams(
    string? Libelle = null, decimal? MinMontant = null, decimal? MaxMontant = null,
    double? MaxPoids = null, DateTime? DateLivraison = null, int? ClientId = null);

public class ColisStatsDto
{
    public int TotalColis { get; init; }
    public decimal TotalMontant { get; init; }
    public Dictionary<string, int> ColisParLivreur { get; init; } = [];
    public Dictionary<string, decimal> MontantParMois { get; init; } = [];
}

public class PagedResult<T>
{
    public List<T> Items     { get; init; } = [];
    public int     TotalCount { get; init; }
    public int     Page       { get; init; }
    public int     PageSize   { get; init; }
    public int     TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}
