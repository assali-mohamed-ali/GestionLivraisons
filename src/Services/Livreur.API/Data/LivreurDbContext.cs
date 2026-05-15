using Livreur.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Livreur.API.Data;

public class LivreurDbContext : DbContext
{
    public LivreurDbContext(DbContextOptions<LivreurDbContext> options) : base(options) { }
    public DbSet<Models.Livreur> Livreurs => Set<Models.Livreur>();
    public DbSet<Vehicule> Vehicules => Set<Vehicule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TPH (Table-per-Hierarchy) for Vehicule hierarchy
        modelBuilder.Entity<Vehicule>()
            .HasDiscriminator<string>("Type")
            .HasValue<Camion>("Camion")
            .HasValue<Voiture>("Voiture");
    }
}
