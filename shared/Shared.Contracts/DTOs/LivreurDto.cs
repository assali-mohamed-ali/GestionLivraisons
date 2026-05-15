namespace Shared.Contracts.DTOs;

public record LivreurDto(
    int Id, string Nom, string Prenom, string CIN,
    string Telephone, string Email, string Ville, string Adresse);

public record CreateLivreurDto(
    string Nom, string Prenom, string CIN,
    string Telephone, string Email, string Ville, string Adresse);

public record LivreurStatsDto(int TotalLivreurs, Dictionary<string, int> LivreursParVille);
