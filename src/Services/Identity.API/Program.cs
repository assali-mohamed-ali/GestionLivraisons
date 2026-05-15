using Identity.API.Data;
using Identity.API.Models;
using Identity.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppIdentityDbContext>(o =>
    o.UseSqlite("Data Source=identity.db"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o =>
{
    o.Password.RequireDigit = true;
    o.Password.RequiredLength = 6;
    o.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppIdentityDbContext>()
.AddDefaultTokenProviders();

// SRP: TokenService only generates tokens
builder.Services.AddScoped<TokenService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
    await db.Database.EnsureCreatedAsync();
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api/auth/login", async (
    LoginRequestDto dto,
    UserManager<ApplicationUser> um,
    SignInManager<ApplicationUser> sm,
    TokenService ts) =>
{
    var user = await um.FindByEmailAsync(dto.Email);
    if (user is null)
        return Results.Ok(new AuthResponseDto { Success = false, Error = "Identifiants incorrects." });

    var signInResult = await sm.CheckPasswordSignInAsync(user, dto.Password, false);
    if (!signInResult.Succeeded)
        return Results.Ok(new AuthResponseDto { Success = false, Error = "Identifiants incorrects." });

    var role  = (await um.GetRolesAsync(user)).FirstOrDefault() ?? "User";
    var token = ts.GenerateToken(user, role);

    return Results.Ok(new AuthResponseDto
    {
        Success = true, Token = token,
        Email = user.Email, FullName = user.FullName, Role = role
    });
});

app.MapPost("/api/auth/register", async (
    RegisterRequestDto dto,
    UserManager<ApplicationUser> um) =>
{
    var user = new ApplicationUser
    {
        UserName = dto.Email, Email = dto.Email,
        FullName = dto.FullName, EmailConfirmed = true
    };
    var result = await um.CreateAsync(user, dto.Password);
    if (!result.Succeeded)
        return Results.BadRequest(new AuthResponseDto
        {
            Success = false,
            Error = string.Join(", ", result.Errors.Select(e => e.Description))
        });

    await um.AddToRoleAsync(user, "User");
    return Results.Ok(new AuthResponseDto
        { Success = true, Email = user.Email, FullName = user.FullName, Role = "User" });
});

app.MapGet("/api/auth/users", async (UserManager<ApplicationUser> um) =>
{
    var result = new List<UserDto>();
    foreach (var u in um.Users.ToList())
    {
        var role = (await um.GetRolesAsync(u)).FirstOrDefault() ?? "User";
        result.Add(new UserDto(u.Id, u.Email!, u.FullName, role));
    }
    return Results.Ok(result);
});

app.MapDelete("/api/auth/users/{id}", async (string id, UserManager<ApplicationUser> um) =>
{
    var user = await um.FindByIdAsync(id);
    return user is null ? Results.NotFound() : Results.Ok(await um.DeleteAsync(user));
});

app.Run();
