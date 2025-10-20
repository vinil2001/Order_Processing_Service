using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Core.Entities
{
    public enum OrderStatus
    {
        Pending = 0,
        Processing = 1,
        Processed = 2,
        Failed = 3
    }
}
