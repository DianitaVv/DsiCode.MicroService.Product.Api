using Microsoft.EntityFrameworkCore;
using DsiCode.MicroService.Product.API.Models;

namespace DsiCode.MicroService.Product.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ✅ Cambiar nombre más claro y usar tipo correcto
        public DbSet<Models.Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ IMPORTANTE: Configurar prefijo para tabla de productos
            // Esto permite usar una sola base de datos para todos los microservicios
            modelBuilder.Entity<Models.Product>().ToTable("Product_Products");

            // ✅ Configuraciones adicionales para Product
            modelBuilder.Entity<Models.Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductId).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(500);

                entity.Property(e => e.ImageLocalPath)
                    .HasMaxLength(500);

                // ✅ Índices para mejorar rendimiento
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.CategoryName);
                entity.HasIndex(e => e.Price);
            });

            // ✅ Seed inicial de productos (opcional)
            modelBuilder.Entity<Models.Product>().HasData(
                new Models.Product
                {
                    ProductId = 1,
                    Name = "Producto de Ejemplo",
                    Price = 99.99,
                    Description = "Producto de ejemplo creado automáticamente",
                    CategoryName = "Ejemplo",
                    ImageUrl = "https://placehold.co/600x400",
                    ImageLocalPath = null
                }
            );
        }
    }
}