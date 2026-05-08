using Bogus;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Data
{
    /// <summary>
    /// Clase DataSeeder que se encarga de inicializar la base de datos con datos de prueba.
    /// </summary>
    public class DataSeeder
    {
        /// <summary>
        /// Método estático que inicializa la base de datos con datos de prueba.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                await context.Database.EnsureCreatedAsync(); // Asegura que la base de datos se cree si no existe
                await context.Database.MigrateAsync(); // Aplica las migraciones pendientes a la base de datos

                // Extraer géneros de usuario desde la configuración
                var genders = configuration.GetSection("User:Genders").Get<string[]>() ?? throw new InvalidOperationException("No se pudieron cargar los géneros de usuario");

                // Creacion de roles
                if (!await context.Roles.AnyAsync())
                {
                    var roles = new List<Role>
                        {
                            new Role { Name = "Admin" },
                            new Role { Name = "Customer" }
                        };
                    await context.Roles.AddRangeAsync(roles);
                    await context.SaveChangesAsync();
                    Log.Information("Roles creados con exito");
                }

                // Creacion de categorias
                if (!await context.Categories.AnyAsync())
                {
                    var categories = new List<Category>
                        {
                            new Category { Name = "Electronica", Description = "Dispositivos electrónicos y gadgets" },
                            new Category { Name = "Ropa", Description = "Indumentaria y moda para todas las edades" },
                            new Category { Name = "Hogar", Description = "Artículos para la decoración y uso doméstico" },
                            new Category { Name = "Juguetes", Description = "Diversión para niños y coleccionistas" },
                            new Category { Name = "Libros", Description = "Literatura, textos académicos y más" }
                        };
                    await context.Categories.AddRangeAsync(categories);
                    await context.SaveChangesAsync();
                    Log.Information("Categorias creadas con exito");
                }

                // Creacion de marcas
                if (!await context.Brands.AnyAsync())
                {
                    var brands = new List<Brand>
                        {
                            new Brand { Name = "Apple", Description = "Empresa tecnológica estadounidense" },
                            new Brand { Name = "Nike", Description = "Ropa y calzado deportivo" },
                            new Brand { Name = "Samsung", Description = "Multinacional surcoreana de tecnología" },
                            new Brand { Name = "Adidas", Description = "Marca deportiva alemana" },
                            new Brand { Name = "Sony", Description = "Entretenimiento y tecnología japonesa" }
                        };
                    await context.Brands.AddRangeAsync(brands);
                    await context.SaveChangesAsync();
                    Log.Information("Marcas creadas con exito");
                }

                // Creacion de usuarios
                if (!await context.Users.AnyAsync())
                {
                    // Obtener los roles
                    Role customerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer") ?? throw new InvalidOperationException("No se pudo encontrar el rol 'Customer'");
                    Role adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin") ?? throw new InvalidOperationException("No se pudo encontrar el rol 'Admin'");

                    // Crear usuario admin
                    User adminUser = new User
                    {
                        Name = configuration["User:AdminUser:Name"] ?? throw new InvalidOperationException("No se pudo cargar el nombre del usuario admin"),
                        Email = configuration["User:AdminUser:Email"] ?? throw new InvalidOperationException("No se pudo cargar el email del usuario admin"),
                        EmailConfirmed = true,
                        Rut = configuration["User:AdminUser:Rut"] ?? throw new InvalidOperationException("No se pudo cargar el RUT del usuario admin"),
                        PhoneNumber = configuration["User:AdminUser:PhoneNumber"] ?? throw new InvalidOperationException("No se pudo cargar el número de teléfono del usuario admin"),
                        BirthDate = DateTime.Parse(configuration["User:AdminUser:BirthDate"] ?? throw new InvalidOperationException("No se pudo cargar la fecha de nacimiento del usuario admin")),
                        Gender = configuration["User:AdminUser:Gender"] ?? throw new InvalidOperationException("No se pudo cargar el género del usuario admin"),
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(configuration["User:AdminUser:Password"] ?? throw new InvalidOperationException("No se pudo cargar el hash de contraseña del usuario admin")),
                        RoleId = adminRole.Id
                    };
                    await context.Users.AddAsync(adminUser);
                    await context.SaveChangesAsync();
                    Log.Information("Usuario admin creado con exito");

                    // Crear usuarios de prueba
                    var randomPasswordHash = BCrypt.Net.BCrypt.HashPassword(configuration["User:RandomUserPassword"] ?? throw new InvalidOperationException("No se pudo cargar la contraseña de los usuarios de prueba"));

                    var userFaker = new Faker<User>()
                        .RuleFor(u => u.Name, f => f.Name.FullName())
                        .RuleFor(u => u.Email, f => f.Internet.Email())
                        .RuleFor(u => u.EmailConfirmed, true)
                        .RuleFor(u => u.Rut, f => RandomRut())
                        .RuleFor(u => u.PhoneNumber, f => RandomPhoneNumber())
                        .RuleFor(u => u.BirthDate, f => f.Date.Past(30, DateTime.Now.AddYears(-18))) // Usuarios mayores de 18 años
                        .RuleFor(u => u.Gender, f => f.PickRandom(genders))
                        .RuleFor(u => u.PasswordHash, randomPasswordHash)
                        .RuleFor(u => u.RoleId, customerRole.Id);

                    var users = userFaker.Generate(99);
                    await context.Users.AddRangeAsync(users);
                    await context.SaveChangesAsync();
                    Log.Information("Usuarios de prueba creados con exito");
                }

                // Creacion de productos e imagenes
                if (!await context.Products.AnyAsync())
                {
                    var categoryIds = await context.Categories.Select(c => c.Id).ToListAsync();
                    var brandIds = await context.Brands.Select(b => b.Id).ToListAsync();

                    var imageFaker = new Faker<Image>()
                        .RuleFor(i => i.ImageUrl, f => f.Image.PicsumUrl())
                        .RuleFor(i => i.PublicId, f => f.Random.Guid().ToString());

                    var productFaker = new Faker<Product>()
                        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                        .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
                        .RuleFor(p => p.Price, f => f.Random.Int(1000, 100000))
                        .RuleFor(p => p.Stock, f => f.Random.Int(0, 100))
                        .RuleFor(p => p.CategoryId, f => f.PickRandom(categoryIds))
                        .RuleFor(p => p.BrandId, f => f.PickRandom(brandIds))
                        .RuleFor(p => p.Images, f => imageFaker.Generate(f.Random.Int(1, 3)));

                    var products = productFaker.Generate(50);
                    await context.Products.AddRangeAsync(products);
                    await context.SaveChangesAsync();
                    Log.Information("Productos de prueba e imagenes creados con exito");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al aplicar migraciones a la base de datos", ex.Message);
            }
        }

        /// <summary>
        /// Genera un RUT chileno aleatorio en formato "XXXXXXXX-X".
        /// </summary>
        /// <returns></returns>
        private static string RandomRut()
        {
            var faker = new Faker();
            var number = faker.Random.Int(1000000, 99999999).ToString();
            var verifier = faker.Random.Int(0, 9).ToString();
            return $"{number}-{verifier}";
        }

        /// <summary>
        /// Genera un número de teléfono chileno aleatorio en formato "+569 XXXX-XXXX".
        /// </summary>
        /// <returns></returns>
        private static string RandomPhoneNumber()
        {
            var faker = new Faker();
            string firstPart = faker.Random.Int(1000, 9999).ToString();
            string secondPart = faker.Random.Int(1000, 9999).ToString();
            return $"+569 {firstPart}-{secondPart}";
        }
    }
}