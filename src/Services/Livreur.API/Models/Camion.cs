using Shared.Contracts.DTOs;

namespace Livreur.API.Models;

// LSP correct subtype — substitutable for Vehicule without type-checking
public class Camion : Vehicule
{
    public double Capacite { get; set; }
    public int NbrEssieux { get; set; }

    public override string GetDescription() =>
        $"Camion {Marque} — {NbrEssieux} essieux — capacité {Capacite} kg";

    public override int GetCapacitePassagers() => 0; // Camion carries no passengers — valid substitution

    public override bool PeutTransporter(double poidsKg) => poidsKg <= Capacite;

    public override VehiculeDto ToDto() =>
        new(Id, "Camion", Matricule, Marque, Couleur, VitesseLimite, LivreurId,
            GetDescription(), Capacite, NbrEssieux, null);
}
