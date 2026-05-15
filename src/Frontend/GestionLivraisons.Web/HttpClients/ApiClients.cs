using GestionLivraisons.Web.HttpClients.Interfaces;
using Shared.Contracts.DTOs;

namespace GestionLivraisons.Web.HttpClients;

public class AuthApiClient(HttpClient http) : IAuthApiClient
{
    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto)
    {
        var response = await http.PostAsJsonAsync("/api/auth/login", dto);
        return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
    }
    public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto dto)
    {
        var response = await http.PostAsJsonAsync("/api/auth/register", dto);
        return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
    }
    public async Task<List<UserDto>?> GetUsersAsync() =>
        await http.GetFromJsonAsync<List<UserDto>>("/api/auth/users");
    public async Task DeleteUserAsync(string id) =>
        await http.DeleteAsync($"/api/auth/users/{id}");
}

public class ColisApiClient(HttpClient http) : IColisApiClient
{
    public async Task<PagedResult<ColisDto>?> SearchAsync(ColisSearchParams search, int page, int pageSize)
    {
        var query = $"/api/colis?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search.Libelle)) query += $"&libelle={search.Libelle}";
        if (search.MinMontant.HasValue) query += $"&minMontant={search.MinMontant}";
        if (search.MaxMontant.HasValue) query += $"&maxMontant={search.MaxMontant}";
        if (search.MaxPoids.HasValue) query += $"&maxPoids={search.MaxPoids}";
        if (search.DateLivraison.HasValue) query += $"&dateLivraison={search.DateLivraison:yyyy-MM-dd}";
        if (search.ClientId.HasValue) query += $"&clientId={search.ClientId}";
        return await http.GetFromJsonAsync<PagedResult<ColisDto>>(query);
    }
    public async Task<PagedResult<ColisDto>?> GetAllAsync() =>
        await http.GetFromJsonAsync<PagedResult<ColisDto>>("/api/colis?pageSize=100");
    public async Task<ColisDto?> GetByIdAsync(int id) =>
        await http.GetFromJsonAsync<ColisDto>($"/api/colis/{id}");
    public async Task<HttpResponseMessage> CreateAsync(CreateColisDto dto) =>
        await http.PostAsJsonAsync("/api/colis", dto);
    public async Task<HttpResponseMessage> UpdateAsync(int id, CreateColisDto dto) =>
        await http.PutAsJsonAsync($"/api/colis/{id}", dto);
    public async Task DeleteAsync(int id) =>
        await http.DeleteAsync($"/api/colis/{id}");
}

public class LivreurApiClient(HttpClient http) : ILivreurApiClient
{
    public async Task<List<LivreurDto>?> GetAllAsync() =>
        await http.GetFromJsonAsync<List<LivreurDto>>("/api/livreurs");
    public async Task<HttpResponseMessage> CreateAsync(CreateLivreurDto dto) =>
        await http.PostAsJsonAsync("/api/livreurs", dto);
    public async Task<HttpResponseMessage> UpdateAsync(int id, CreateLivreurDto dto) =>
        await http.PutAsJsonAsync($"/api/livreurs/{id}", dto);
    public async Task DeleteAsync(int id) =>
        await http.DeleteAsync($"/api/livreurs/{id}");
    public async Task<List<VehiculeDto>?> GetVehiculesAsync() =>
        await http.GetFromJsonAsync<List<VehiculeDto>>("/api/vehicules");
    public async Task<HttpResponseMessage> CreateVehiculeAsync(CreateVehiculeDto dto) =>
        await http.PostAsJsonAsync("/api/vehicules", dto);
    public async Task DeleteVehiculeAsync(int id) =>
        await http.DeleteAsync($"/api/vehicules/{id}");
}

public class ClientApiClient(HttpClient http) : IClientApiClient
{
    public async Task<List<ClientDto>?> GetAllAsync() =>
        await http.GetFromJsonAsync<List<ClientDto>>("/api/clients");
    public async Task<ClientDto?> GetByEmailAsync(string email)
    {
        var response = await http.GetAsync($"/api/clients/email/{email}");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<ClientDto>();
        return null;
    }
    public async Task<HttpResponseMessage> CreateAsync(CreateClientDto dto) =>
        await http.PostAsJsonAsync("/api/clients", dto);
    public async Task<HttpResponseMessage> UpdateAsync(int id, CreateClientDto dto) =>
        await http.PutAsJsonAsync($"/api/clients/{id}", dto);
    public async Task DeleteAsync(int id) =>
        await http.DeleteAsync($"/api/clients/{id}");
}

public class DashboardApiClient(HttpClient http) : IDashboardApiClient
{
    public async Task<DashboardDto?> GetDashboardAsync() =>
        await http.GetFromJsonAsync<DashboardDto>("/api/dashboard");
}
