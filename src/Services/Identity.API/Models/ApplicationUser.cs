using Microsoft.AspNetCore.Identity;

namespace Identity.API.Models;

// LSP: ApplicationUser extends IdentityUser — any code accepting IdentityUser works with ApplicationUser
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
