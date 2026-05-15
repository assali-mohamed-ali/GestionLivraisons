using Livreur.API.Services.Interfaces;
using Livreur.API.UnitOfWork;
using Shared.Contracts.DTOs;

namespace Livreur.API.Services;

// SRP: only business rules for Livreur
// DIP: depends on ILivreurUnitOfWork (abstraction)
public class LivreurService(ILivreurUnitOfWork unitOfWork) : ILivreurAdminService
{
    public async Task<IReadOnlyList<LivreurDto>> GetAllAsync()
    {
        var livreurs = await unitOfWork.Livreurs.GetAllAsync();
        return livreurs.Select(MapToDto).ToList();
    }

    public async Task<LivreurDto?> GetByIdAsync(int id)
    {
        var livreur = await unitOfWork.Livreurs.GetByIdAsync(id);
        return livreur is null ? null : MapToDto(livreur);
    }

    public async Task<LivreurDto> CreateAsync(CreateLivreurDto dto)
    {
        var livreur = new Models.Livreur
        {
            Nom = dto.Nom,
            Prenom = dto.Prenom,
            CIN = dto.CIN,
            Telephone = dto.Telephone,
            Email = dto.Email,
            Ville = dto.Ville,
            Adresse = dto.Adresse
        };
        await unitOfWork.Livreurs.AddAsync(livreur);
        await unitOfWork.SaveAsync();
        return MapToDto(livreur);
    }

    public async Task UpdateAsync(int id, CreateLivreurDto dto)
    {
        var livreur = await unitOfWork.Livreurs.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Livreur {id} introuvable.");
        livreur.Nom = dto.Nom;
        livreur.Prenom = dto.Prenom;
        livreur.CIN = dto.CIN;
        livreur.Telephone = dto.Telephone;
        livreur.Email = dto.Email;
        livreur.Ville = dto.Ville;
        livreur.Adresse = dto.Adresse;
        unitOfWork.Livreurs.Update(livreur);
        await unitOfWork.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var livreur = await unitOfWork.Livreurs.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Livreur {id} introuvable.");
        unitOfWork.Livreurs.Delete(livreur);
        await unitOfWork.SaveAsync();
    }

    public async Task<LivreurStatsDto> GetStatsAsync()
    {
        var livreurs = await unitOfWork.Livreurs.GetAllAsync();
        return new LivreurStatsDto(
            livreurs.Count,
            livreurs.GroupBy(l => l.Ville).ToDictionary(g => g.Key, g => g.Count()));
    }

    private static LivreurDto MapToDto(Models.Livreur l) =>
        new(l.Id, l.Nom, l.Prenom, l.CIN, l.Telephone, l.Email, l.Ville, l.Adresse);
}
