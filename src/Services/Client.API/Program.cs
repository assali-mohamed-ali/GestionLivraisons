using Client.API.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ClientDbContext>(o => o.UseSqlite("Data Source=client.db"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClientDbContext>();
    await db.Database.EnsureCreatedAsync();
    ClientSeeder.Seed(db);
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/clients", async (ClientDbContext db) =>
    Results.Ok(await db.Clients.Select(c =>
        new ClientDto(c.Id, c.Nom, c.Prenom, c.Telephone, c.Email, c.Ville, c.Adresse)).ToListAsync()));

app.MapGet("/api/clients/{id:int}", async (int id, ClientDbContext db) =>
{
    var c = await db.Clients.FindAsync(id);
    return c is null ? Results.NotFound() :
        Results.Ok(new ClientDto(c.Id, c.Nom, c.Prenom, c.Telephone, c.Email, c.Ville, c.Adresse));
});

app.MapGet("/api/clients/email/{email}", async (string email, ClientDbContext db) =>
{
    var c = await db.Clients.FirstOrDefaultAsync(x => x.Email == email);
    return c is null ? Results.NotFound() :
        Results.Ok(new ClientDto(c.Id, c.Nom, c.Prenom, c.Telephone, c.Email, c.Ville, c.Adresse));
});

app.MapGet("/api/clients/stats", async (ClientDbContext db) =>
{
    var clients = await db.Clients.ToListAsync();
    return Results.Ok(new ClientStatsDto(
        clients.Count,
        clients.GroupBy(c => c.Ville).ToDictionary(g => g.Key, g => g.Count())));
});

app.MapPost("/api/clients", async (CreateClientDto dto, ClientDbContext db) =>
{
    var client = new Client.API.Models.Client
    {
        Nom = dto.Nom, Prenom = dto.Prenom, Telephone = dto.Telephone,
        Email = dto.Email, Ville = dto.Ville, Adresse = dto.Adresse
    };
    db.Clients.Add(client);
    await db.SaveChangesAsync();
    return Results.Created($"/api/clients/{client.Id}",
        new ClientDto(client.Id, client.Nom, client.Prenom, client.Telephone, client.Email, client.Ville, client.Adresse));
});

app.MapPut("/api/clients/{id:int}", async (int id, CreateClientDto dto, ClientDbContext db) =>
{
    var client = await db.Clients.FindAsync(id);
    if (client is null) return Results.NotFound();
    client.Nom = dto.Nom; client.Prenom = dto.Prenom; client.Telephone = dto.Telephone;
    client.Email = dto.Email; client.Ville = dto.Ville; client.Adresse = dto.Adresse;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/clients/{id:int}", async (int id, ClientDbContext db) =>
{
    var client = await db.Clients.FindAsync(id);
    if (client is null) return Results.NotFound();
    db.Clients.Remove(client);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
