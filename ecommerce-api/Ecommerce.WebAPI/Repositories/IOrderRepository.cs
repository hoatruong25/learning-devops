using Ecommerce.WebAPI.Models;

namespace Ecommerce.WebAPI.Repositories;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task<Order> CreateAsync(Order order);
    Task<Order?> UpdateStatusAsync(int id, OrderStatus status);
}
