using GestionLivraisons.Web.HttpClients.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.DTOs;

namespace GestionLivraisons.Web.Controllers;

// DIP: depends on interfaces, not concrete HttpClient classes
// SRP: only HTTP orchestration — no business logic
[Authorize(Roles = "Admin")]
public class AdminController(
    ILivreurApiClient livreurClient,
    IClientApiClient  clientClient,
    IColisApiClient   colisClient,
    IAuthApiClient    authClient) : Controller
{
    // Livreurs
    public async Task<IActionResult> Livreurs()
        => View(await livreurClient.GetAllAsync() ?? new List<LivreurDto>());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLivreur(CreateLivreurDto dto)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Données invalides."; return RedirectToAction(nameof(Livreurs)); }
        var r = await livreurClient.CreateAsync(dto);
        TempData[r.IsSuccessStatusCode ? "Success" : "Error"] =
            r.IsSuccessStatusCode ? "Livreur créé." : "Erreur création livreur.";
        return RedirectToAction(nameof(Livreurs));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLivreur(int id, CreateLivreurDto dto)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Données invalides."; return RedirectToAction(nameof(Livreurs)); }
        var r = await livreurClient.UpdateAsync(id, dto);
        TempData[r.IsSuccessStatusCode ? "Success" : "Error"] =
            r.IsSuccessStatusCode ? "Livreur modifié." : "Erreur modification.";
        return RedirectToAction(nameof(Livreurs));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLivreur(int id)
    {
        await livreurClient.DeleteAsync(id);
        TempData["Success"] = "Livreur supprimé.";
        return RedirectToAction(nameof(Livreurs));
    }

    // Vehicules — uses VehiculeFactory server-side via Livreur.API
    public async Task<IActionResult> Vehicules()
    {
        ViewBag.Livreurs = await livreurClient.GetAllAsync() ?? new List<LivreurDto>();
        return View(await livreurClient.GetVehiculesAsync() ?? new List<VehiculeDto>());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVehicule(CreateVehiculeDto dto)
    {
        var r = await livreurClient.CreateVehiculeAsync(dto);
        if (!r.IsSuccessStatusCode)
        {
            TempData["Error"] = "Validation échouée: " + await r.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(Vehicules));
        }
        TempData["Success"] = "Véhicule créé.";
        return RedirectToAction(nameof(Vehicules));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVehicule(int id)
    {
        await livreurClient.DeleteVehiculeAsync(id);
        TempData["Success"] = "Véhicule supprimé.";
        return RedirectToAction(nameof(Vehicules));
    }

    // Clients
    public async Task<IActionResult> Clients()
        => View(await clientClient.GetAllAsync() ?? new List<ClientDto>());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClient(CreateClientDto dto)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Données invalides."; return RedirectToAction(nameof(Clients)); }
        var r = await clientClient.CreateAsync(dto);
        TempData[r.IsSuccessStatusCode ? "Success" : "Error"] =
            r.IsSuccessStatusCode ? "Client créé." : "Erreur création client.";
        return RedirectToAction(nameof(Clients));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditClient(int id, CreateClientDto dto)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Données invalides."; return RedirectToAction(nameof(Clients)); }
        var r = await clientClient.UpdateAsync(id, dto);
        TempData[r.IsSuccessStatusCode ? "Success" : "Error"] =
            r.IsSuccessStatusCode ? "Client modifié." : "Erreur modification.";
        return RedirectToAction(nameof(Clients));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteClient(int id)
    {
        await clientClient.DeleteAsync(id);
        TempData["Success"] = "Client supprimé.";
        return RedirectToAction(nameof(Clients));
    }

    // Colis admin
    public async Task<IActionResult> Colis()
    {
        ViewBag.Livreurs = await livreurClient.GetAllAsync() ?? new List<LivreurDto>();
        ViewBag.Clients = await clientClient.GetAllAsync() ?? new List<ClientDto>();
        return View(await colisClient.GetAllAsync() ?? new PagedResult<ColisDto>());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateColis(CreateColisDto dto)
    {
        var r = await colisClient.CreateAsync(dto);
        TempData[r.IsSuccessStatusCode ? "Success" : "Error"] =
            r.IsSuccessStatusCode ? "Colis créé." : "Erreur création colis.";
        return RedirectToAction(nameof(Colis));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditColis(int id, CreateColisDto dto)
    {
        var r = await colisClient.UpdateAsync(id, dto);
        TempData[r.IsSuccessStatusCode ? "Success" : "Error"] =
            r.IsSuccessStatusCode ? "Colis modifié." : "Erreur modification.";
        return RedirectToAction(nameof(Colis));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteColis(int id)
    {
        await colisClient.DeleteAsync(id);
        TempData["Success"] = "Colis supprimé.";
        return RedirectToAction(nameof(Colis));
    }

    // Comptes
    public async Task<IActionResult> Comptes()
        => View(await authClient.GetUsersAsync() ?? new List<UserDto>());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCompte(string id)
    {
        await authClient.DeleteUserAsync(id);
        TempData["Success"] = "Compte supprimé.";
        return RedirectToAction(nameof(Comptes));
    }
}
