using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Core.Interfaces
{
    public interface IOrderProcessor
    {
        Task ProcessAsync(Guid orderId, CancellationToken cancellationToken = default);
    }
}
