using System.ComponentModel.DataAnnotations;
using Ecommerce.WebAPI.DTOs;
using Ecommerce.WebAPI.Models;
using Ecommerce.WebAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ILogger<OrdersController> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll()
    {
        var orders = await _orderRepository.GetAllAsync();
        return Ok(orders);
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetById(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            _logger.LogWarning("Order with ID {OrderId} not found", id);
            return NotFound(new { message = $"Order with ID {id} not found" });
        }

        return Ok(order);
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody] CreateOrderRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Validate all products exist and calculate total
        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", item.ProductId);
                return BadRequest(new { message = $"Product with ID {item.ProductId} not found" });
            }

            if (product.Stock < item.Quantity)
            {
                _logger.LogWarning("Insufficient stock for product {ProductId}. Requested: {Quantity}, Available: {Stock}",
                    item.ProductId, item.Quantity, product.Stock);
                return BadRequest(new { message = $"Insufficient stock for product {product.Name}. Available: {product.Stock}" });
            }

            var orderItem = new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            };

            orderItems.Add(orderItem);
            totalAmount += product.Price * item.Quantity;

            // Update stock
            product.Stock -= item.Quantity;
            await _productRepository.UpdateAsync(product.Id, product);
        }

        var order = new Order
        {
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            TotalAmount = totalAmount,
            Items = orderItems
        };

        var createdOrder = await _orderRepository.CreateAsync(order);
        _logger.LogInformation("Created order {OrderId} for customer {CustomerName} with total {TotalAmount:C}",
            createdOrder.Id, createdOrder.CustomerName, createdOrder.TotalAmount);

        return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
    }

    /// <summary>
    /// Update order status
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<Order>> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedOrder = await _orderRepository.UpdateStatusAsync(id, request.Status);
        if (updatedOrder == null)
        {
            _logger.LogWarning("Cannot update status - Order with ID {OrderId} not found", id);
            return NotFound(new { message = $"Order with ID {id} not found" });
        }

        _logger.LogInformation("Updated order {OrderId} status to {Status}", updatedOrder.Id, updatedOrder.Status);
        return Ok(updatedOrder);
    }
}

public class UpdateOrderStatusRequest
{
    [EnumDataType(typeof(OrderStatus))]
    public OrderStatus Status { get; set; }
}
