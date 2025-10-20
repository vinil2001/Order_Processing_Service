using OrderProcessingService.Core.Entities;

namespace OrderProcessingService.Core.Interfaces
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order, CancellationToken cancellationToken = default);
        Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task UpdateStatusAsync(Guid id, OrderStatus status, DateTime? processedAt = null, CancellationToken cancellationToken = default);
    }
}
