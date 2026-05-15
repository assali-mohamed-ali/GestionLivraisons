namespace Shared.Contracts.DTOs;

public record VehiculeDto(
    int Id, string Type, string Matricule, string Marque,
    string Couleur, int VitesseLimite, int LivreurId,
    string Description, double? Capacite, int? NbrEssieux, int? NbrPlaces);

public record CreateVehiculeDto(
    string Type, string Matricule, string Marque,
    string Couleur, int VitesseLimite, int? LivreurId,
    double? Capacite, int? NbrEssieux, int? NbrPlaces);
