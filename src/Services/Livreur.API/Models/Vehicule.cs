using Shared.Contracts.DTOs;

namespace Livreur.API.Models;

// LSP: every subtype provides its own correct implementation
// Callers use Vehicule, never need to cast
public abstract class Vehicule
{
    public int Id { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public string Marque { get; set; } = string.Empty;
    public string Couleur { get; set; } = string.Empty;
    public int VitesseLimite { get; set; }
    public int LivreurId { get; set; }

    // LSP: every subtype provides its own correct implementation
    public abstract string GetDescription();
    public abstract int GetCapacitePassagers();
    public abstract bool PeutTransporter(double poidsKg);

    // DTO mapping — polymorphic
    public abstract VehiculeDto ToDto();
}
