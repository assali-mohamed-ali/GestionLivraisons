using GestionLivraisons.Web.HttpClients.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.DTOs;
using System.Security.Claims;

namespace GestionLivraisons.Web.Controllers;

[Authorize]
public class ColisController(IColisApiClient colisClient, IClientApiClient clientClient, ILivreurApiClient livreurClient) : Controller
{
    public async Task<IActionResult> Index(
        string? libelle, decimal? minMontant, decimal? maxMontant,
        double? maxPoids, DateTime? dateLivraison, int page = 1)
    {
        int? clientId = null;
        if (!User.IsInRole("Admin"))
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var client = await clientClient.GetByEmailAsync(email);
                if (client != null) clientId = client.Id;
                else clientId = -1; // Utilisateur sans profil client -> 0 résultats
            }
        }

        var search = new ColisSearchParams(libelle, minMontant, maxMontant, maxPoids, dateLivraison, clientId);
        var result = await colisClient.SearchAsync(search, page, 10);
        ViewBag.Search = search;
        ViewBag.Clients = await clientClient.GetAllAsync() ?? new List<ClientDto>();
        ViewBag.Livreurs = await livreurClient.GetAllAsync() ?? new List<LivreurDto>();
        return View(result ?? new PagedResult<ColisDto>());
    }

    public async Task<IActionResult> Details(int id)
    {
        var colis = await colisClient.GetByIdAsync(id);
        if (colis is null) return NotFound();
        ViewBag.Clients = await clientClient.GetAllAsync() ?? new List<ClientDto>();
        ViewBag.Livreurs = await livreurClient.GetAllAsync() ?? new List<LivreurDto>();
        return View(colis);
    }
}
