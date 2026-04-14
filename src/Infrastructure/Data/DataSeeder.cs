using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Domain.Models;

using Bogus;

namespace TiendaUCN.src.Infrastructure.Data
{
    public static class DataSeeder
    {
        /// <summary>
        /// Genera una lista de usuarios ficticios utilizando Bogus.
        /// </summary>
        /// <param name="quantity">Cantidad de usuarios a generar.</param>
        /// <returns>Lista de usuarios generados.</returns>
        public static List<User> GenerateUsers(int quantity = 10)
        {
            var genders = new[] { "Masculino", "Femenino" };

            var users = new Faker<User>("es")
                .RuleFor(u => u.Name, f => f.Person.FullName)
                .RuleFor(u => u.Rut, f => f.Random.Replace("##.###.###-#"))
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber("+56 9 #### ####"))
                .RuleFor(u => u.BirthDate, f => f.Date.Past(30, DateTime.Now.AddYears(-18)))
                .RuleFor(u => u.Gender, f => f.PickRandom(genders))
                .RuleFor(u => u.PasswordHash, f => BCrypt.Net.BCrypt.HashPassword("User" + f.Random.Number(1000, 9999)))
                .RuleFor(u => u.IsDeleted, false)
                .Generate(quantity);

            return users;
        }

        /// <summary>
        /// Inicializa la base de datos con migraciones, roles y usuarios.
        /// </summary>
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new DataContext(
                serviceProvider.GetRequiredService<DbContextOptions<DataContext>>());

            await context.Database.MigrateAsync();

            // Seed Roles
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Name = "Admin" },
                    new Role { Name = "User" }
                );

                await context.SaveChangesAsync();
            }

            // Seed Users
            if (!context.Users.Any())
            {
                var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
                var userRole = await context.Roles.FirstAsync(r => r.Name == "User");

                // Crear usuario administrador
                var admin = new User
                {
                    Name = "Administrador",
                    Email = "admin@tiendaucn.cl",
                    Rut = "11.111.111-1",
                    PhoneNumber = "+56 9 9999 9999",
                    BirthDate = new DateTime(1990, 1, 1),
                    Gender = "Masculino",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pa$$word2025"),
                    EmailConfirmed = true,
                    RoleId = adminRole.Id,
                    IsDeleted = false
                };

                context.Users.Add(admin);

                // Crear usuarios ficticios con rol "User"
                var fakeUsers = GenerateUsers(10);
                foreach (var user in fakeUsers)
                {
                    user.RoleId = userRole.Id;
                }

                context.Users.AddRange(fakeUsers);

                await context.SaveChangesAsync();
            }
        }
    }
}
