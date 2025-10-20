using OrderProcessingService.Core.DTOs;
using OrderProcessingService.Core.Entities;
using OrderProcessingService.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Core.Services
{
    public sealed class OrderProcessingService
    {
        private readonly IOrderRepository _repository;
        private readonly IOrderQueue _queue;

        public OrderProcessingService(IOrderRepository repository, IOrderQueue queue)
        {
            _repository = repository;
            _queue = queue;
        }

        public async Task<OrderResponse> SubmitAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
        {
            var order = new Order
            {
                CustomerId = request.CustomerId,
                Items = request.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList(),
                TotalAmount = request.TotalAmount,
                Status = OrderStatus.Pending
            };

            if (order.TotalAmount <= 0m)
            {
                order.TotalAmount = order.Items.Sum(x => x.UnitPrice * x.Quantity);
            }

            await _repository.AddAsync(order, cancellationToken);
            await _queue.EnqueueAsync(order.Id, cancellationToken);

            return new OrderResponse
            {
                OrderId = order.Id,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                ProcessedAt = order.ProcessedAt
            };
        }
    }
}
