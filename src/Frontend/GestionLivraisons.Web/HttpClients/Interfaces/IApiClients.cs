using Shared.Contracts.DTOs;

namespace GestionLivraisons.Web.HttpClients.Interfaces;

public interface IAuthApiClient
{
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto);
    Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto dto);
    Task<List<UserDto>?> GetUsersAsync();
    Task DeleteUserAsync(string id);
}

public interface IColisApiClient
{
    Task<PagedResult<ColisDto>?> SearchAsync(ColisSearchParams search, int page, int pageSize);
    Task<PagedResult<ColisDto>?> GetAllAsync();
    Task<ColisDto?> GetByIdAsync(int id);
    Task<HttpResponseMessage> CreateAsync(CreateColisDto dto);
    Task<HttpResponseMessage> UpdateAsync(int id, CreateColisDto dto);
    Task DeleteAsync(int id);
}

public interface ILivreurApiClient
{
    Task<List<LivreurDto>?> GetAllAsync();
    Task<HttpResponseMessage> CreateAsync(CreateLivreurDto dto);
    Task<HttpResponseMessage> UpdateAsync(int id, CreateLivreurDto dto);
    Task DeleteAsync(int id);
    Task<List<VehiculeDto>?> GetVehiculesAsync();
    Task<HttpResponseMessage> CreateVehiculeAsync(CreateVehiculeDto dto);
    Task DeleteVehiculeAsync(int id);
}

public interface IClientApiClient
{
    Task<List<ClientDto>?> GetAllAsync();
    Task<ClientDto?> GetByEmailAsync(string email);
    Task<HttpResponseMessage> CreateAsync(CreateClientDto dto);
    Task<HttpResponseMessage> UpdateAsync(int id, CreateClientDto dto);
    Task DeleteAsync(int id);
}

public interface IDashboardApiClient
{
    Task<DashboardDto?> GetDashboardAsync();
}
