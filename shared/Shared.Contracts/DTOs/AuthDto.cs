namespace Shared.Contracts.DTOs;

public record LoginRequestDto(string Email, string Password);
public record RegisterRequestDto(string Email, string Password, string FullName);

public class AuthResponseDto
{
    public bool    Success  { get; init; }
    public string? Token    { get; init; }
    public string? Email    { get; init; }
    public string? FullName { get; init; }
    public string? Role     { get; init; }
    public string? Error    { get; init; }
}

public record UserDto(string Id, string Email, string FullName, string Role);
