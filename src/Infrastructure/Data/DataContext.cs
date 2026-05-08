using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Data
{
    /// <summary>
    /// Clase DataContext que representa el contexto de datos.
    /// </summary>
    public class DataContext : DbContext
    {
        /// <summary>
        /// Constrcutor de DataContext
        /// </summary>
        /// <param name="options">Opciones del contexto de datos</param>
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// DbSet de Usuarios
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;

        /// <summary>
        /// DbSet de Roles
        /// </summary>
        public DbSet<Role> Roles { get; set; } = null!;

        /// <summary>
        /// DbSet de Códigos de Verificación
        /// </summary>
        public DbSet<VerificationCode> VerificationCodes { get; set; } = null!;

        /// <summary>
        /// DbSet de Productos
        /// </summary>
        public DbSet<Product> Products { get; set; } = null!;

        /// <summary>
        /// DbSet de Imágenes
        /// </summary>
        public DbSet<Image> Images { get; set; } = null!;

        /// <summary>
        /// DbSet de Marcas
        /// </summary>
        public DbSet<Brand> Brands { get; set; } = null!;

        /// <summary>
        /// DbSet de Categorías
        /// </summary>
        public DbSet<Category> Categories { get; set; } = null!;

        /// <summary>
        /// DbSet de Carritos de compra
        /// </summary>
        public DbSet<Cart> Carts { get; set; } = null!;

        /// <summary>
        /// DbSet de Items del Carrito
        /// </summary>
        public DbSet<CartItem> CartItems { get; set; } = null!;

        /// <summary>
        /// DbSet de Pedidos
        /// </summary>
        public DbSet<Order> Orders { get; set; } = null!;

        /// <summary>
        /// DbSet de Items del Pedido
        /// </summary>
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        /// <summary>
        /// DbSet de Tokens en Lista Negra
        /// </summary>
        public DbSet<BlackListedToken> BlacklistedTokens { get; set; } = null!;
    }
}