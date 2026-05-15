namespace Colis.API.Data;

// SRP: ColisSeeder only seeds data
public static class ColisSeeder
{
    public static void Seed(ColisDbContext context)
    {
        if (context.Colis.Any()) return;

        var random = new Random(42);
        var libelles = new[] { "Colis Express", "Paquet Standard", "Envoi Fragile", "Colis Volumineux", "Petit Paquet", "Documents", "Colis Alimentaire", "Matériel IT" };

        for (int i = 1; i <= 50; i++)
        {
            context.Colis.Add(new Models.Colis
            {
                Libelle = $"{libelles[random.Next(libelles.Length)]} #{i}",
                DateLivraison = DateTime.Today.AddDays(-random.Next(0, 90)),
                Montant = Math.Round((decimal)(random.NextDouble() * 500 + 10), 2),
                Poids = Math.Round(random.NextDouble() * 50 + 0.5, 2),
                Volume = Math.Round(random.NextDouble() * 2 + 0.1, 2),
                LivreurId = random.Next(1, 6),
                ClientId = random.Next(1, 11)
            });
        }
        context.SaveChanges();
    }
}
