using Ecommerce.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.WebAPI.Data;

public class EcommerceDbContext : DbContext
{
    public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId);

        // Seed demo data
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Laptop Dell XPS 13",
                Description = "High-performance ultrabook with 13-inch display",
                Price = 1299.99m,
                Stock = 15,
                Category = "Electronics",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 2,
                Name = "iPhone 15 Pro",
                Description = "Latest Apple smartphone with advanced camera",
                Price = 999.99m,
                Stock = 25,
                Category = "Electronics",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 3,
                Name = "Sony WH-1000XM5",
                Description = "Premium noise-canceling wireless headphones",
                Price = 399.99m,
                Stock = 30,
                Category = "Audio",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 4,
                Name = "Nike Air Max 270",
                Description = "Comfortable running shoes with air cushioning",
                Price = 149.99m,
                Stock = 50,
                Category = "Footwear",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 5,
                Name = "The Lean Startup Book",
                Description = "Business methodology book by Eric Ries",
                Price = 19.99m,
                Stock = 100,
                Category = "Books",
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
