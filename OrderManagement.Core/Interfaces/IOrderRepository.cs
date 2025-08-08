using OrderManagement.Core.Entities;

namespace OrderManagement.Core.Interfaces;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order);
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order> UpdateAsync(Order order);
    Task DeleteAsync(Guid id);
}