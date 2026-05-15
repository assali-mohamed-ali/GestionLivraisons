using GestionLivraisons.Web.HttpClients.Interfaces;
using GestionLivraisons.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.DTOs;
using System.Security.Claims;

namespace GestionLivraisons.Web.Controllers;

// DIP: depends on IAuthApiClient, not AuthApiClient
public class AccountController(IAuthApiClient authClient) : Controller
{
    [HttpGet] public IActionResult Login() => View(new LoginVM());
    [HttpGet] public IActionResult Register() => View(new RegisterVM());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM model)
    {
        if (!ModelState.IsValid) return View(model);

        var response = await authClient.LoginAsync(
            new LoginRequestDto(model.Email, model.Password));

        if (response?.Success != true)
        {
            ModelState.AddModelError("", response?.Error ?? "Échec de la connexion.");
            return View(model);
        }

        // Store JWT for outbound API calls (AuthTokenHandler reads this)
        HttpContext.Session.SetString("jwt_token", response.Token!);

        // Create cookie identity for MVC [Authorize]
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name,  response.FullName!),
            new(ClaimTypes.Email, response.Email!),
            new(ClaimTypes.Role,  response.Role!),
        };
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme)));

        return response.Role == "Admin"
            ? RedirectToAction("Index", "Dashboard")
            : RedirectToAction("Index", "Colis");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVM model)
    {
        if (!ModelState.IsValid) return View(model);
        var response = await authClient.RegisterAsync(
            new RegisterRequestDto(model.Email, model.Password, model.FullName));
        if (response?.Success != true)
        {
            ModelState.AddModelError("", response?.Error ?? "Erreur lors de l'inscription.");
            return View(model);
        }
        TempData["Success"] = "Compte créé. Connectez-vous.";
        return RedirectToAction(nameof(Login));
    }

    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Remove("jwt_token");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
