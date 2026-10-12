using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FFS.Models
{
    //готовые заказы(FIFO).Официант забирает
    public class ServiceQueue
    {
        private ConcurrentQueue<OrderTicket> tickets = new ConcurrentQueue<OrderTicket>();
        private SemaphoreSlim signal = new SemaphoreSlim(0);

        public int Count => tickets.Count;

        public List<int> GetNumbers()
        {
            return tickets.Select(t => t.Number).ToList();
        }

        public void Add(OrderTicket ticket)
        {
            tickets.Enqueue(ticket);
            signal.Release();
        }

        public async Task<OrderTicket> TakeNext(CancellationToken token)
        {
            await signal.WaitAsync(token);
            tickets.TryDequeue(out OrderTicket ticket);
            return ticket;
        }
    }
}