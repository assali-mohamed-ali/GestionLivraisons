using Microsoft.EntityFrameworkCore;

namespace Client.API.Data;

public class ClientDbContext : DbContext
{
    public ClientDbContext(DbContextOptions<ClientDbContext> options) : base(options) { }
    public DbSet<Models.Client> Clients => Set<Models.Client>();
}
