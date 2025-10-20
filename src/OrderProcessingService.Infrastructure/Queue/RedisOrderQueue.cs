using OrderProcessingService.Core.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Infrastructure.Queue
{
    public class RedisOrderQueue : IOrderQueue
    {
        private readonly IDatabase _db;
        private readonly string _listKey = "orders:queue";

        public RedisOrderQueue(IConnectionMultiplexer mux)
        {
            _db = mux.GetDatabase();
        }

        public Task EnqueueAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return _db.ListRightPushAsync(_listKey, orderId.ToString());
        }

        public async Task<Guid?> DequeueAsync(CancellationToken cancellationToken = default)
        {
            // просте опитування (polling) з невеликою паузою
            // (BRPOP у StackExchange.Redis не має тонкої обгортки, можна було б через Script/Eval)
            while (!cancellationToken.IsCancellationRequested)
            {
                var value = await _db.ListLeftPopAsync(_listKey);
                if (!value.IsNullOrEmpty && Guid.TryParse(value!, out var id))
                    return id;

                await Task.Delay(500, cancellationToken); // backoff
            }
            return null;
        }
    }
}
