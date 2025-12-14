using Ecommerce.WebAPI.DTOs;
using Ecommerce.WebAPI.Models;
using Ecommerce.WebAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductRepository productRepository, ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        var products = await _productRepository.GetAllAsync();
        return Ok(products);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found", id);
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        return Ok(product);
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<Product>>> GetByCategory(string category)
    {
        var products = await _productRepository.GetByCategoryAsync(category);
        return Ok(products);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            Category = request.Category
        };

        var createdProduct = await _productRepository.CreateAsync(product);
        _logger.LogInformation("Created product {ProductId}: {ProductName}", createdProduct.Id, createdProduct.Name);

        return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Product>> Update(int id, [FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            Category = request.Category
        };

        var updatedProduct = await _productRepository.UpdateAsync(id, product);
        if (updatedProduct == null)
        {
            _logger.LogWarning("Cannot update - Product with ID {ProductId} not found", id);
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        _logger.LogInformation("Updated product {ProductId}: {ProductName}", updatedProduct.Id, updatedProduct.Name);
        return Ok(updatedProduct);
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _productRepository.DeleteAsync(id);
        if (!deleted)
        {
            _logger.LogWarning("Cannot delete - Product with ID {ProductId} not found", id);
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        _logger.LogInformation("Deleted product {ProductId}", id);
        return NoContent();
    }
}
