using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Core.Entities;
using OrderProcessingService.Core.Interfaces;
using OrderProcessingService.Infrastructure.Data;

namespace OrderProcessingService.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _db;

    public OrderRepository(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Orders
              .Include(o => o.Items)
              .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task UpdateStatusAsync(Guid id, OrderStatus status, DateTime? processedAt = null, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (order is null) return;
        order.Status = status;
        order.ProcessedAt = processedAt;
        await _db.SaveChangesAsync(cancellationToken);
    }
}