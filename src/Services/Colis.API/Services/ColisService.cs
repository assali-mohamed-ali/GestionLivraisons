using Colis.API.Services.Interfaces;
using Colis.API.UnitOfWork;
using Shared.Contracts.DTOs;

namespace Colis.API.Services;

// SRP: only business rules for Colis — no HTTP, no EF queries directly
// DIP: depends on IUnitOfWork (abstraction), not UnitOfWork (concrete)
public class ColisService(IUnitOfWork unitOfWork) : IColisAdminService
{
    public async Task<PagedResult<ColisDto>> SearchAsync(ColisSearchParams p, int page = 1, int pageSize = 10)
    {
        var (items, total) = await unitOfWork.Colis.SearchPagedAsync(p, page, pageSize);

        return new PagedResult<ColisDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ColisDto?> GetByIdAsync(int id)
    {
        var colis = await unitOfWork.Colis.GetByIdAsync(id);
        return colis is null ? null : MapToDto(colis);
    }

    public async Task<ColisDto> CreateAsync(CreateColisDto dto)
    {
        var colis = new Models.Colis
        {
            Libelle = dto.Libelle,
            DateLivraison = dto.DateLivraison,
            Montant = dto.Montant,
            Poids = dto.Poids,
            Volume = dto.Volume,
            LivreurId = dto.LivreurId,
            ClientId = dto.ClientId
        };
        await unitOfWork.Colis.AddAsync(colis);
        await unitOfWork.SaveAsync();
        return MapToDto(colis);
    }

    public async Task UpdateAsync(int id, CreateColisDto dto)
    {
        var colis = await unitOfWork.Colis.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Colis {id} introuvable.");
        colis.Libelle = dto.Libelle;
        colis.DateLivraison = dto.DateLivraison;
        colis.Montant = dto.Montant;
        colis.Poids = dto.Poids;
        colis.Volume = dto.Volume;
        colis.LivreurId = dto.LivreurId;
        colis.ClientId = dto.ClientId;
        unitOfWork.Colis.Update(colis);
        await unitOfWork.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var colis = await unitOfWork.Colis.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Colis {id} introuvable.");
        unitOfWork.Colis.Delete(colis);
        await unitOfWork.SaveAsync();
    }

    public async Task<ColisStatsDto> GetStatsAsync()
    {
        var stats = await unitOfWork.Colis.GetStatsAsync();
        return new ColisStatsDto
        {
            TotalColis = stats.TotalColis,
            TotalMontant = stats.TotalMontant,
            ColisParLivreur = stats.ColisParLivreur,
            MontantParMois = stats.MontantParMois
        };
    }

    private static ColisDto MapToDto(Models.Colis c) =>
        new(c.Id, c.Libelle, c.DateLivraison, c.Montant, c.Poids, c.Volume,
            c.LivreurId, null, c.ClientId, null);
}
