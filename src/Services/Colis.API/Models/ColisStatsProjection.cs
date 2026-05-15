namespace Colis.API.Models;

public class ColisStatsProjection
{
    public int TotalColis { get; set; }
    public decimal TotalMontant { get; set; }
    public Dictionary<string, int> ColisParLivreur { get; set; } = [];
    public Dictionary<string, decimal> MontantParMois { get; set; } = [];
}
