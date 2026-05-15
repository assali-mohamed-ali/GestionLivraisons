namespace Colis.API.Models;

public class Colis
{
    public int Id { get; set; }
    public string Libelle { get; set; } = string.Empty;
    public DateTime DateLivraison { get; set; }
    public decimal Montant { get; set; }
    public double Poids { get; set; }
    public double Volume { get; set; }
    public int LivreurId { get; set; }
    public int ClientId { get; set; }
}
