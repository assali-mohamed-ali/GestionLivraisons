using Colis.API.Data;
using Colis.API.Repositories;
using Colis.API.Repositories.Interfaces;
using Colis.API.Search;
using Colis.API.Services;
using Colis.API.Services.Interfaces;
using Colis.API.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ColisDbContext>(o => o.UseSqlite("Data Source=colis.db"));

// OCP: register all search strategies — add new ones without changing engine
builder.Services.AddScoped<ISearchStrategy<Colis.API.Models.Colis>, LibelleSearchStrategy>();
builder.Services.AddScoped<ISearchStrategy<Colis.API.Models.Colis>, MontantRangeStrategy>();
builder.Services.AddScoped<ISearchStrategy<Colis.API.Models.Colis>, PoidsMaxStrategy>();
builder.Services.AddScoped<ISearchStrategy<Colis.API.Models.Colis>, DateLivraisonStrategy>();
builder.Services.AddScoped<ISearchStrategy<Colis.API.Models.Colis>, ClientIdStrategy>();
builder.Services.AddScoped<ColisSearchEngine>();

// Repository + UoW (DIP: bind abstractions to implementations)
builder.Services.AddScoped<IColisRepository, ColisRepository>();
builder.Services.AddScoped<IUnitOfWork, Colis.API.UnitOfWork.UnitOfWork>();

// ISP: bind admin and read service to same implementation
builder.Services.AddScoped<IColisAdminService, ColisService>();
builder.Services.AddScoped<IColisReadService>(sp => sp.GetRequiredService<IColisAdminService>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ColisDbContext>();
    await db.Database.EnsureCreatedAsync();
    ColisSeeder.Seed(db);
}

app.UseSwagger();
app.UseSwaggerUI();

// Endpoints inject IColisAdminService (DIP — not ColisService directly)
app.MapGet("/api/colis", async (
    IColisReadService svc,
    string? libelle, decimal? minMontant, decimal? maxMontant,
    double? maxPoids, DateTime? dateLivraison, int? clientId,
    int page = 1, int pageSize = 10) =>
{
    var p = new ColisSearchParams(libelle, minMontant, maxMontant, maxPoids, dateLivraison, clientId);
    return Results.Ok(await svc.SearchAsync(p, page, pageSize));
});

app.MapGet("/api/colis/{id:int}", async (int id, IColisReadService svc) =>
    await svc.GetByIdAsync(id) is { } dto ? Results.Ok(dto) : Results.NotFound());

app.MapGet("/api/colis/stats", async (IColisAdminService svc) =>
    Results.Ok(await svc.GetStatsAsync()));

app.MapPost("/api/colis", async (CreateColisDto dto, IColisAdminService svc) =>
{
    var created = await svc.CreateAsync(dto);
    return Results.Created($"/api/colis/{created.Id}", created);
});

app.MapPut("/api/colis/{id:int}", async (int id, CreateColisDto dto, IColisAdminService svc) =>
{
    await svc.UpdateAsync(id, dto);
    return Results.NoContent();
});

app.MapDelete("/api/colis/{id:int}", async (int id, IColisAdminService svc) =>
{
    await svc.DeleteAsync(id);
    return Results.NoContent();
});

app.Run();
