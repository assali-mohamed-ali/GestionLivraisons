using Dashboard.API.Clients;
using Shared.Contracts.DTOs;

var builder = WebApplication.CreateBuilder(args);

// DIP: typed HTTP clients for each downstream API
builder.Services.AddHttpClient<IColisApiClient, ColisApiClient>(c =>
    c.BaseAddress = new Uri(builder.Configuration["Services:ColisApi"]!));

builder.Services.AddHttpClient<ILivreurApiClient, LivreurApiClient>(c =>
    c.BaseAddress = new Uri(builder.Configuration["Services:LivreurApi"]!));

builder.Services.AddHttpClient<IClientApiClient, ClientApiClient>(c =>
    c.BaseAddress = new Uri(builder.Configuration["Services:ClientApi"]!));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/dashboard", async (
    IColisApiClient colisClient,
    ILivreurApiClient livreurClient,
    IClientApiClient clientClient) =>
{
    // Aggregate data from all microservices in parallel
    var colisStatsTask = colisClient.GetStatsAsync();
    var livreurStatsTask = livreurClient.GetStatsAsync();
    var clientStatsTask = clientClient.GetStatsAsync();
    var recentTask = colisClient.GetRecentAsync(10);

    await Task.WhenAll(colisStatsTask, livreurStatsTask, clientStatsTask, recentTask);

    var colisStats = await colisStatsTask;
    var livreurStats = await livreurStatsTask;
    var clientStats = await clientStatsTask;
    var recent = await recentTask;

    return Results.Ok(new DashboardDto
    {
        TotalColis      = colisStats?.TotalColis ?? 0,
        TotalMontant    = colisStats?.TotalMontant ?? 0,
        TotalLivreurs   = livreurStats?.TotalLivreurs ?? 0,
        TotalClients    = clientStats?.TotalClients ?? 0,
        ColisParLivreur = colisStats?.ColisParLivreur ?? [],
        ColisParVille   = clientStats?.ClientsParVille ?? [],
        MontantParMois  = colisStats?.MontantParMois ?? [],
        ColisRecents    = recent ?? []
    });
});

app.Run();
