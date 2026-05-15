using Livreur.API.Models;

namespace Livreur.API.Data;

// SRP: LivreurSeeder only seeds data
public static class LivreurSeeder
{
    public static void Seed(LivreurDbContext context)
    {
        if (context.Livreurs.Any()) return;

        var livreurs = new[]
        {
            new Models.Livreur { Nom = "Benali", Prenom = "Ahmed", CIN = "AB123456", Telephone = "0612345678", Email = "ahmed@livraisons.com", Ville = "Casablanca", Adresse = "Rue 1, Maarif" },
            new Models.Livreur { Nom = "Idrissi", Prenom = "Fatima", CIN = "CD789012", Telephone = "0698765432", Email = "fatima@livraisons.com", Ville = "Rabat", Adresse = "Av. Hassan II" },
            new Models.Livreur { Nom = "Ouahbi", Prenom = "Youssef", CIN = "EF345678", Telephone = "0654321098", Email = "youssef@livraisons.com", Ville = "Marrakech", Adresse = "Derb Sultan" },
            new Models.Livreur { Nom = "Tazi", Prenom = "Khadija", CIN = "GH901234", Telephone = "0676543210", Email = "khadija@livraisons.com", Ville = "Fès", Adresse = "Bab Boujloud" },
            new Models.Livreur { Nom = "Alaoui", Prenom = "Omar", CIN = "IJ567890", Telephone = "0632109876", Email = "omar@livraisons.com", Ville = "Tanger", Adresse = "Bd Mohamed V" },
        };
        context.Livreurs.AddRange(livreurs);
        context.SaveChanges();

        var vehicules = new Vehicule[]
        {
            new Camion { Matricule = "CAM-001", Marque = "Mercedes", Couleur = "Blanc", VitesseLimite = 90, LivreurId = 1, Capacite = 5000, NbrEssieux = 4 },
            new Camion { Matricule = "CAM-002", Marque = "Volvo", Couleur = "Bleu", VitesseLimite = 80, LivreurId = 2, Capacite = 8000, NbrEssieux = 6 },
            new Voiture { Matricule = "VOI-001", Marque = "Dacia", Couleur = "Rouge", VitesseLimite = 130, LivreurId = 3, NbrPlaces = 5 },
            new Voiture { Matricule = "VOI-002", Marque = "Renault", Couleur = "Noir", VitesseLimite = 140, LivreurId = 4, NbrPlaces = 5 },
            new Camion { Matricule = "CAM-003", Marque = "MAN", Couleur = "Vert", VitesseLimite = 85, LivreurId = 5, Capacite = 10000, NbrEssieux = 8 },
        };
        context.Vehicules.AddRange(vehicules);
        context.SaveChanges();
    }
}
