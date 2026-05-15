using Shared.Contracts.DTOs;

namespace Livreur.API.Models;

// LSP correct subtype — substitutable for Vehicule without type-checking
public class Voiture : Vehicule
{
    public int NbrPlaces { get; set; }

    public override string GetDescription() =>
        $"Voiture {Marque} — {NbrPlaces} places";

    public override int GetCapacitePassagers() => NbrPlaces;

    public override bool PeutTransporter(double poidsKg) => poidsKg <= 500; // Voiture limit

    public override VehiculeDto ToDto() =>
        new(Id, "Voiture", Matricule, Marque, Couleur, VitesseLimite, LivreurId,
            GetDescription(), null, null, NbrPlaces);
}
