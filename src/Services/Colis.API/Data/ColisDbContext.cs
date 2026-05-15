using Microsoft.EntityFrameworkCore;

namespace Colis.API.Data;

public class ColisDbContext : DbContext
{
    public ColisDbContext(DbContextOptions<ColisDbContext> options) : base(options) { }
    public DbSet<Models.Colis> Colis => Set<Models.Colis>();
}
