using Livreur.API.Data;
using Livreur.API.Factories;
using Livreur.API.Repositories;
using Livreur.API.Repositories.Interfaces;
using Livreur.API.Services;
using Livreur.API.Services.Interfaces;
using Livreur.API.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LivreurDbContext>(o => o.UseSqlite("Data Source=livreur.db"));

// DIP: bind abstractions
builder.Services.AddScoped<ILivreurRepository, LivreurRepository>();
builder.Services.AddScoped<IVehiculeRepository, VehiculeRepository>();
builder.Services.AddScoped<ILivreurUnitOfWork, LivreurUnitOfWork>();

// ISP: split admin vs read service
builder.Services.AddScoped<ILivreurAdminService, LivreurService>();
builder.Services.AddScoped<ILivreurReadService>(sp => sp.GetRequiredService<ILivreurAdminService>());

// Factory — OCP + ISP + SRP (Singleton: stateless, thread-safe)
builder.Services.AddSingleton<IVehiculeFactory, VehiculeFactory>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LivreurDbContext>();
    await db.Database.EnsureCreatedAsync();
    LivreurSeeder.Seed(db);
}

app.UseSwagger();
app.UseSwaggerUI();

// Livreur endpoints
app.MapGet("/api/livreurs", async (ILivreurReadService svc) =>
    Results.Ok(await svc.GetAllAsync()));

app.MapGet("/api/livreurs/{id:int}", async (int id, ILivreurReadService svc) =>
    await svc.GetByIdAsync(id) is { } dto ? Results.Ok(dto) : Results.NotFound());

app.MapGet("/api/livreurs/stats", async (ILivreurAdminService svc) =>
    Results.Ok(await svc.GetStatsAsync()));

app.MapPost("/api/livreurs", async (CreateLivreurDto dto, ILivreurAdminService svc) =>
{
    var created = await svc.CreateAsync(dto);
    return Results.Created($"/api/livreurs/{created.Id}", created);
});

app.MapPut("/api/livreurs/{id:int}", async (int id, CreateLivreurDto dto, ILivreurAdminService svc) =>
{
    await svc.UpdateAsync(id, dto);
    return Results.NoContent();
});

app.MapDelete("/api/livreurs/{id:int}", async (int id, ILivreurAdminService svc) =>
{
    await svc.DeleteAsync(id);
    return Results.NoContent();
});

// Vehicule endpoints — Factory pattern used here
app.MapGet("/api/vehicules", async (ILivreurUnitOfWork uow) =>
    Results.Ok((await uow.Vehicules.GetAllAsync()).Select(v => v.ToDto())));

app.MapPost("/api/vehicules", async (
    CreateVehiculeDto dto,
    IVehiculeFactory factory,      // ISP: IVehiculeCreator + IVehiculeValidator
    ILivreurUnitOfWork uow) =>
{
    // Validate first (IVehiculeValidator)
    var validation = factory.Validate(dto);
    if (!validation.IsValid)
        return Results.BadRequest(new { Errors = validation.Errors });

    // Create via factory (IVehiculeCreator) — LSP: result is always usable as Vehicule
    var vehicule = factory.Create(dto);
    await uow.Vehicules.AddAsync(vehicule);
    await uow.SaveAsync();
    return Results.Created($"/api/vehicules/{vehicule.Id}", vehicule.ToDto());
});

app.MapDelete("/api/vehicules/{id:int}", async (int id, ILivreurUnitOfWork uow) =>
{
    var v = await uow.Vehicules.GetByIdAsync(id);
    if (v is null) return Results.NotFound();
    uow.Vehicules.Delete(v);
    await uow.SaveAsync();
    return Results.NoContent();
});

app.Run();
