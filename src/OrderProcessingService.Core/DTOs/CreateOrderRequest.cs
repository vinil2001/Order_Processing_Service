using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Core.DTOs
{
    public sealed class CreateOrderRequest
    {
        public Guid CustomerId { get; set; }
        public List<CreateOrderItem> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }
}
