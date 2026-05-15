namespace Livreur.API.Models;

public class Livreur
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string CIN { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;

    // Navigation
    public List<Vehicule> Vehicules { get; set; } = [];
}
