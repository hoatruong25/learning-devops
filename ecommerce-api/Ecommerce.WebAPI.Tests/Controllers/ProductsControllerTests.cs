using Ecommerce.WebAPI.Controllers;
using Ecommerce.WebAPI.DTOs;
using Ecommerce.WebAPI.Models;
using Ecommerce.WebAPI.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ecommerce.WebAPI.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<ILogger<ProductsController>> _mockLogger;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.99m, Category = "Electronics" },
            new() { Id = 2, Name = "Product 2", Price = 20.99m, Category = "Books" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<Product>>().Subject;
        returnedProducts.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsProduct()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Price = 15.99m, Category = "Electronics" };
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProduct = okResult.Value.Should().BeAssignableTo<Product>().Subject;
        returnedProduct.Id.Should().Be(1);
        returnedProduct.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByCategory_ReturnsFilteredProducts()
    {
        // Arrange
        var electronicsProducts = new List<Product>
        {
            new() { Id = 1, Name = "Laptop", Price = 999.99m, Category = "Electronics" },
            new() { Id = 2, Name = "Phone", Price = 599.99m, Category = "Electronics" }
        };
        _mockRepository.Setup(r => r.GetByCategoryAsync("Electronics")).ReturnsAsync(electronicsProducts);

        // Act
        var result = await _controller.GetByCategory("Electronics");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var products = okResult.Value.Should().BeAssignableTo<IEnumerable<Product>>().Subject;
        products.Should().HaveCount(2);
        products.Should().AllSatisfy(p => p.Category.Should().Be("Electronics"));
    }

    [Fact]
    public async Task Create_WithValidProduct_ReturnsCreatedProduct()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "New Product",
            Description = "Test Description",
            Price = 25.99m,
            Stock = 10,
            Category = "Books"
        };
        var createdProduct = new Product
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            Category = request.Category
        };
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Product>())).ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var product = createdResult.Value.Should().BeAssignableTo<Product>().Subject;
        product.Name.Should().Be("New Product");
        product.Price.Should().Be(25.99m);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithValidId_ReturnsUpdatedProduct()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 35.99m,
            Stock = 20,
            Category = "Electronics"
        };
        var updatedProduct = new Product
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            Category = request.Category
        };
        _mockRepository.Setup(r => r.UpdateAsync(1, It.IsAny<Product>())).ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.Update(1, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var product = okResult.Value.Should().BeAssignableTo<Product>().Subject;
        product.Name.Should().Be("Updated Product");
        _mockRepository.Verify(r => r.UpdateAsync(1, It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Updated Product",
            Price = 35.99m,
            Stock = 20,
            Category = "Electronics"
        };
        _mockRepository.Setup(r => r.UpdateAsync(999, It.IsAny<Product>())).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.Update(999, request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
