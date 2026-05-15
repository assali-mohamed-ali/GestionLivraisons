namespace Shared.Contracts.DTOs;

public class DashboardDto
{
    public int                       TotalColis      { get; init; }
    public decimal                   TotalMontant    { get; init; }
    public int                       TotalLivreurs   { get; init; }
    public int                       TotalClients    { get; init; }
    public Dictionary<string, int>   ColisParLivreur { get; init; } = [];
    public Dictionary<string, int>   ColisParVille   { get; init; } = [];
    public Dictionary<string, decimal> MontantParMois { get; init; } = [];
    public List<ColisDto>            ColisRecents    { get; init; } = [];
}
