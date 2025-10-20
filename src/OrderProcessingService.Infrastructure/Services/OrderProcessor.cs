using Microsoft.Extensions.Logging;
using OrderProcessingService.Core.Entities;
using OrderProcessingService.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Infrastructure.Services
{
    public class OrderProcessor : IOrderProcessor
    {
        private readonly IOrderRepository _repo;
        private readonly ILogger<OrderProcessor> _logger;

        public OrderProcessor(IOrderRepository repo, ILogger<OrderProcessor> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task ProcessAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            var order = await _repo.GetByIdAsync(orderId, cancellationToken);
            if (order is null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                return;
            }

            try
            {
                await _repo.UpdateStatusAsync(orderId, OrderStatus.Processing, null, cancellationToken);

                // Симуляція бізнес-логіки: валідації/знижки/затримка
                if (order.Items.Count == 0 || order.TotalAmount <= 0)
                    throw new InvalidOperationException("Invalid order");

                await Task.Delay(1000, cancellationToken); // імітуємо роботу

                await _repo.UpdateStatusAsync(orderId, OrderStatus.Processed, DateTime.UtcNow, cancellationToken);
                _logger.LogInformation("Order {OrderId} processed", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process order {OrderId}", orderId);
                await _repo.UpdateStatusAsync(orderId, OrderStatus.Failed, DateTime.UtcNow, cancellationToken);
            }
        }
    }
}
