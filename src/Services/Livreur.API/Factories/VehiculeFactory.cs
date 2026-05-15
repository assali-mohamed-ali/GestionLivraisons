using Livreur.API.Models;
using Shared.Contracts.DTOs;

namespace Livreur.API.Factories;

// OCP + SRP: Concrete Factory — extend by adding new vehicle types
public class VehiculeFactory : IVehiculeFactory
{
    // IVehiculeValidator
    public ValidationResult Validate(CreateVehiculeDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Matricule)) errors.Add("Matricule est requis.");
        if (string.IsNullOrWhiteSpace(dto.Marque))    errors.Add("Marque est requise.");
        if (dto.VitesseLimite <= 0)                   errors.Add("Vitesse limite doit être positive.");

        errors.AddRange(dto.Type switch
        {
            "Camion"  => ValidateCamion(dto),
            "Voiture" => ValidateVoiture(dto),
            _         => [$"Type de véhicule inconnu: '{dto.Type}'."]
        });

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure([.. errors]);
    }

    private static IEnumerable<string> ValidateCamion(CreateVehiculeDto dto)
    {
        if (!dto.Capacite.HasValue || dto.Capacite <= 0) yield return "Capacité requise pour un Camion.";
        if (!dto.NbrEssieux.HasValue || dto.NbrEssieux < 2) yield return "NbrEssieux minimum 2 pour un Camion.";
    }

    private static IEnumerable<string> ValidateVoiture(CreateVehiculeDto dto)
    {
        if (!dto.NbrPlaces.HasValue || dto.NbrPlaces < 1) yield return "NbrPlaces requis pour une Voiture.";
    }

    // IVehiculeCreator — OCP: add new type here without modifying existing branches
    public Vehicule Create(CreateVehiculeDto dto) => dto.Type switch
    {
        "Camion"  => new Camion
        {
            Couleur      = dto.Couleur,
            Marque       = dto.Marque,
            Matricule    = dto.Matricule,
            VitesseLimite= dto.VitesseLimite,
            LivreurId    = dto.LivreurId ?? 0,
            Capacite     = dto.Capacite!.Value,
            NbrEssieux   = dto.NbrEssieux!.Value,
        },
        "Voiture" => new Voiture
        {
            Couleur      = dto.Couleur,
            Marque       = dto.Marque,
            Matricule    = dto.Matricule,
            VitesseLimite= dto.VitesseLimite,
            LivreurId    = dto.LivreurId ?? 0,
            NbrPlaces    = dto.NbrPlaces!.Value,
        },
        _ => throw new InvalidOperationException($"Type inconnu: {dto.Type}. Valider avant de créer.")
    };
}
