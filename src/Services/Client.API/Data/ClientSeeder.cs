namespace Client.API.Data;

public static class ClientSeeder
{
    public static void Seed(ClientDbContext context)
    {
        if (context.Clients.Any()) return;

        context.Clients.AddRange(
            new Models.Client { Nom = "Berrada", Prenom = "Samir", Telephone = "0611223344", Email = "user@livraisons.com", Ville = "Casablanca", Adresse = "Bd Anfa" },
            new Models.Client { Nom = "El Fassi", Prenom = "Nadia", Telephone = "0622334455", Email = "nadia@client.com", Ville = "Rabat", Adresse = "Av. Fal Ould Oumeir" },
            new Models.Client { Nom = "Chraibi", Prenom = "Hassan", Telephone = "0633445566", Email = "hassan@client.com", Ville = "Marrakech", Adresse = "Rue Bab Agnaou" },
            new Models.Client { Nom = "Lahlou", Prenom = "Imane", Telephone = "0644556677", Email = "imane@client.com", Ville = "Fès", Adresse = "Rue Talaa Kbira" },
            new Models.Client { Nom = "Bennani", Prenom = "Karim", Telephone = "0655667788", Email = "karim@client.com", Ville = "Tanger", Adresse = "Av. des FAR" },
            new Models.Client { Nom = "Amrani", Prenom = "Salma", Telephone = "0666778899", Email = "salma@client.com", Ville = "Casablanca", Adresse = "Rue Moulay Youssef" },
            new Models.Client { Nom = "Zaki", Prenom = "Mehdi", Telephone = "0677889900", Email = "mehdi@client.com", Ville = "Rabat", Adresse = "Av. Allal Ben Abdallah" },
            new Models.Client { Nom = "Fassi-Fihri", Prenom = "Amina", Telephone = "0688990011", Email = "amina@client.com", Ville = "Marrakech", Adresse = "Av. Mohammed VI" },
            new Models.Client { Nom = "Senhaji", Prenom = "Younes", Telephone = "0699001122", Email = "younes@client.com", Ville = "Fès", Adresse = "Bd Chefchaouni" },
            new Models.Client { Nom = "Kadiri", Prenom = "Laila", Telephone = "0610112233", Email = "laila@client.com", Ville = "Tanger", Adresse = "Rue de la Liberté" }
        );
        context.SaveChanges();
    }
}
