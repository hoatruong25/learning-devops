using Ecommerce.WebAPI.Data;
using Ecommerce.WebAPI.Models;
using Ecommerce.WebAPI.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.WebAPI.Tests.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly EcommerceDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<EcommerceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new EcommerceDbContext(options);
        _repository = new ProductRepository(_context);

        // Seed test data
        SeedData();
    }

    private void SeedData()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Laptop", Price = 999.99m, Stock = 10, Category = "Electronics" },
            new() { Id = 2, Name = "Book", Price = 19.99m, Stock = 50, Category = "Books" },
            new() { Id = 3, Name = "Headphones", Price = 99.99m, Stock = 25, Category = "Electronics" }
        };
        _context.Products.AddRange(products);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsProduct()
    {
        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Laptop");
        result.Price.Should().Be(999.99m);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnsFilteredProducts()
    {
        // Act
        var result = await _repository.GetByCategoryAsync("Electronics");

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Category.Should().Be("Electronics"));
    }

    [Fact]
    public async Task CreateAsync_AddsNewProduct()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = "Mouse",
            Description = "Wireless mouse",
            Price = 29.99m,
            Stock = 100,
            Category = "Electronics"
        };

        // Act
        var result = await _repository.CreateAsync(newProduct);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Mouse");
        
        var allProducts = await _repository.GetAllAsync();
        allProducts.Should().HaveCount(4);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingProduct()
    {
        // Arrange
        var updatedProduct = new Product
        {
            Name = "Updated Laptop",
            Description = "Updated description",
            Price = 1299.99m,
            Stock = 5,
            Category = "Electronics"
        };

        // Act
        var result = await _repository.UpdateAsync(1, updatedProduct);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Laptop");
        result.Price.Should().Be(1299.99m);
        result.Stock.Should().Be(5);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var updatedProduct = new Product { Name = "Test", Price = 10m, Stock = 1, Category = "Test" };

        // Act
        var result = await _repository.UpdateAsync(999, updatedProduct);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        // Act
        var result = await _repository.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        
        var allProducts = await _repository.GetAllAsync();
        allProducts.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
