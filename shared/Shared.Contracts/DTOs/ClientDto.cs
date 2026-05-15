namespace Shared.Contracts.DTOs;

public record ClientDto(
    int Id, string Nom, string Prenom,
    string Telephone, string Email, string Ville, string Adresse);

public record CreateClientDto(
    string Nom, string Prenom,
    string Telephone, string Email, string Ville, string Adresse);

public record ClientStatsDto(int TotalClients, Dictionary<string, int> ClientsParVille);
