
namespace OrderProcessingService.Core.Interfaces
{
    public interface IOrderQueue
    {
        Task EnqueueAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<Guid?> DequeueAsync(CancellationToken cancellationToken = default);
    }
}
