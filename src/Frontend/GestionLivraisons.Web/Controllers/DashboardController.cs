using GestionLivraisons.Web.HttpClients.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.DTOs;

namespace GestionLivraisons.Web.Controllers;

// SRP: only fetches and passes DashboardDto to View — zero data processing
[Authorize(Roles = "Admin")]
public class DashboardController(IDashboardApiClient dashboardClient) : Controller
{
    public async Task<IActionResult> Index()
        => View(await dashboardClient.GetDashboardAsync() ?? new DashboardDto());
}
