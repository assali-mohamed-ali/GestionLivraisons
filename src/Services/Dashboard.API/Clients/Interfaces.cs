using Shared.Contracts.DTOs;

namespace Dashboard.API.Clients;

// DIP: Dashboard.API's high-level logic depends on abstractions
public interface IColisApiClient
{
    Task<ColisStatsDto?> GetStatsAsync();
    Task<List<ColisDto>?> GetRecentAsync(int count);
}

public interface ILivreurApiClient
{
    Task<LivreurStatsDto?> GetStatsAsync();
}

public interface IClientApiClient
{
    Task<ClientStatsDto?> GetStatsAsync();
}
