using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderProcessingService.Core.Interfaces;
using System.Diagnostics.Metrics;

namespace OrderProcessingService.Worker.Workers;

public class OrderProcessingWorker : BackgroundService
{
    private readonly IOrderQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderProcessingWorker> _logger;
    private static readonly Meter Meter = new("OrderProcessingService");
    private static readonly Counter<long> ProcessedCounter = Meter.CreateCounter<long>("orders.processed");

    public OrderProcessingWorker(
        IOrderQueue queue,
        ILogger<OrderProcessingWorker> logger,
        IServiceScopeFactory scopeFactory
    )
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started");
        while (!stoppingToken.IsCancellationRequested)
        {
            var orderId = await _queue.DequeueAsync(stoppingToken);
            if (orderId is null) continue;

            using var scope = _scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IOrderProcessor>();
            await processor.ProcessAsync(orderId.Value, stoppingToken);

            ProcessedCounter.Add(1);
        }
    }
}