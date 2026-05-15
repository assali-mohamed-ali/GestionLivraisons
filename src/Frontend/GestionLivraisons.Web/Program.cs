using GestionLivraisons.Web.HttpClients;
using GestionLivraisons.Web.HttpClients.Interfaces;
using GestionLivraisons.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout    = TimeSpan.FromHours(8);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// Cookie auth for MVC [Authorize] — populated from JWT claims on login
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath        = "/Account/Login";
        o.AccessDeniedPath = "/Account/Login";
        o.ExpireTimeSpan   = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

// SRP: AuthTokenHandler only injects Bearer token — nothing else
builder.Services.AddTransient<AuthTokenHandler>();

var gatewayUrl = new Uri(builder.Configuration["Gateway:BaseUrl"]!);

// DIP: Controllers depend on IXxxApiClient interfaces, not concrete classes
builder.Services
    .AddHttpClient<IAuthApiClient, AuthApiClient>(c => c.BaseAddress = gatewayUrl)
    .AddHttpMessageHandler<AuthTokenHandler>();

builder.Services
    .AddHttpClient<IColisApiClient, ColisApiClient>(c => c.BaseAddress = gatewayUrl)
    .AddHttpMessageHandler<AuthTokenHandler>();

builder.Services
    .AddHttpClient<ILivreurApiClient, LivreurApiClient>(c => c.BaseAddress = gatewayUrl)
    .AddHttpMessageHandler<AuthTokenHandler>();

builder.Services
    .AddHttpClient<IClientApiClient, ClientApiClient>(c => c.BaseAddress = gatewayUrl)
    .AddHttpMessageHandler<AuthTokenHandler>();

builder.Services
    .AddHttpClient<IDashboardApiClient, DashboardApiClient>(c => c.BaseAddress = gatewayUrl)
    .AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Account}/{action=Login}/{id?}");
app.Run();
