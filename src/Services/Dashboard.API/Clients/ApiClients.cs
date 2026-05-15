using Shared.Contracts.DTOs;

namespace Dashboard.API.Clients;

// DIP: Low-level implementations depend on HttpClient (also an abstraction)
public class ColisApiClient(HttpClient http) : IColisApiClient
{
    public async Task<ColisStatsDto?> GetStatsAsync() =>
        await http.GetFromJsonAsync<ColisStatsDto>("/api/colis/stats");

    public async Task<List<ColisDto>?> GetRecentAsync(int count)
    {
        var result = await http.GetFromJsonAsync<PagedResult<ColisDto>>($"/api/colis?pageSize={count}");
        return result?.Items;
    }
}

public class LivreurApiClient(HttpClient http) : ILivreurApiClient
{
    public async Task<LivreurStatsDto?> GetStatsAsync() =>
        await http.GetFromJsonAsync<LivreurStatsDto>("/api/livreurs/stats");
}

public class ClientApiClient(HttpClient http) : IClientApiClient
{
    public async Task<ClientStatsDto?> GetStatsAsync() =>
        await http.GetFromJsonAsync<ClientStatsDto>("/api/clients/stats");
}
