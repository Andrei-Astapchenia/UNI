using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFS.Models;

namespace FFS.Simulation
{
    public class SimulationEngine
    {
        public KitchenQueue Kitchen { get; } = new KitchenQueue();
        public ServiceQueue Service { get; } = new ServiceQueue();

        public List<Customer> Customers { get; } = new List<Customer>();
        public List<Customer> OrderQueue { get; } = new List<Customer>();

        public List<Cashier> Cashiers { get; } = new List<Cashier>();
        public List<OrderTicket> CookingTickets { get; } = new List<OrderTicket>();//готовящиеся заказы

        public List<OrderTicket> ServingTickets { get; } = new List<OrderTicket>();//выдающиеся

        public readonly object CustomersLock = new object();//лок для потоков
        private CancellationTokenSource cancelSource;
        private int lastCustomerId = 0;
        public int CashierInterval { get; set; } = 4000;
        public int CookInterval { get; set; } = 7000;
        public int ServeInterval { get; set; } = 2000;

        // Координаты зон
        public const double QueueX = 30, QueueY = 100, QueueW = 130, QueueH = 480;
        public const double CashierX = 180, CashierY = 100, CashierW = 180, CashierH = 480;
        public const double WaitingX = 380, WaitingY = 100, WaitingW = 420, WaitingH = 250;
        public const double ServingX = 820, ServingY = 100, ServingW = 200, ServingH = 250;
        public const double KitchenX = 400, KitchenY = 350, KitchenW = 650, KitchenH = 180;

        public event Action StateChanged;

        // Очереди заказов для UI
        public List<int> KitchenWaitingNumbers => Kitchen.GetOrderNumbers();
        public List<int> ServingWaitingNumbers => Service.GetNumbers();

        public void Start(int customerInterval, int cashierInterval, int cookInterval, int serveInterval, int cookCount, int serverCount, int cashierCount)
        {
            cancelSource = new CancellationTokenSource();
            CashierInterval = cashierInterval;
            CookInterval = cookInterval;
            ServeInterval = serveInterval;

            Cashiers.Clear();
            for (int i = 0; i < cashierCount; i++)
                Cashiers.Add(new Cashier { Id = i + 1 });

            for (int i = 0; i < cookCount; i++)
                Task.Run(() => CookWork(cancelSource.Token));

            for (int i = 0; i < serverCount; i++)
                Task.Run(() => ServerWork(cancelSource.Token));

            Task.Run(() => GenerateCustomers(customerInterval, cancelSource.Token));
        }

        public void Stop()
        {
            cancelSource?.Cancel();
            foreach (var c in Customers) c.Ticket?.Cancel();
        }

        private async Task GenerateCustomers(int interval, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    lastCustomerId++;
                    Customer customer = new Customer(lastCustomerId);
                    lock (CustomersLock)
                    {
                        Customers.Add(customer);
                    }
                    OrderQueue.Add(customer);
                    UpdateQueuePositions();
                    StateChanged?.Invoke();
                    Task.Run(() => CustomerLife(customer, token));
                    await Task.Delay(interval, token);
                }
            }
            catch (OperationCanceledException) { }
        }

        private void UpdateQueuePositions()
        {
            for (int i = 0; i < OrderQueue.Count; i++)
            {
                Customer c = OrderQueue[i];
                if (c.State != CustomerState.InQueue &&
                    c.State != CustomerState.WalkingToCashier)
                    continue;
                c.TargetX = QueueX + 40;
                c.TargetY = QueueY + 50 + i * 50;
            }
        }

        private Cashier FindFreeCashier()
        {
            lock (Cashiers)
            {
                return Cashiers.FirstOrDefault(c => !c.IsBusy);
            }
        }
        private async Task CustomerLife(Customer customer, CancellationToken token)
        {
            try
            {
                //Ждёт в очереди
                Cashier cashier = null;
                while (!token.IsCancellationRequested)
                {
                    if (OrderQueue.Count > 0 && OrderQueue[0] == customer)
                    {
                        cashier = FindFreeCashier();
                        if (cashier != null)
                        {
                            lock (Cashiers) { cashier.CurrentCustomer = customer; }
                            break;
                        }
                    }
                    await Task.Delay(100, token);
                }
                if (token.IsCancellationRequested || cashier == null) return;

                //Идём к кассиру
                customer.State = CustomerState.WalkingToCashier;
                double cashierY = CashierY + 80 + (cashier.Id - 1) * 200;
                customer.TargetX = CashierX + 90;
                customer.TargetY = cashierY;
                StateChanged?.Invoke();

                await Task.Delay(800, token);
                
                //создаём заказ
                customer.State = CustomerState.AtCashier;
                OrderTicket ticket = new OrderTicket();
                customer.Ticket = ticket;
                cashier.CurrentTicket = ticket;
                OrderQueue.Remove(customer);
                UpdateQueuePositions();
                StateChanged?.Invoke();
                await Task.Delay(CashierInterval, token);
                //Кассир кладёт заказ на кухню
                Kitchen.Add(ticket);
                cashier.CurrentTicket = null;
                cashier.CurrentCustomer = null;
                StateChanged?.Invoke();
                //в зону ожидания
                customer.State = CustomerState.WaitingForFood;
                int slot = customer.Id % 12;
                customer.TargetX = WaitingX + 60 + (slot % 4) * 100;
                customer.TargetY = WaitingY + 60 + (slot / 4) * 60;
                StateChanged?.Invoke();
                // Ждём Promise официанта
                await ticket.PickupDone.Task;
                //Идём на выдачу
                customer.State = CustomerState.WaitingForPickup;
                customer.TargetX = ServingX + 100;
                customer.TargetY = ServingY + 120;
                StateChanged?.Invoke();
                await Task.Delay(1500, token);

                //Получил заказ
                customer.State = CustomerState.Leaving;
                customer.TargetX = 1200;
                customer.TargetY = customer.Y;
                StateChanged?.Invoke();
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
        }

        private async Task CookWork(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    OrderTicket ticket = await Kitchen.TakeNext(token);
                    lock (CookingTickets) CookingTickets.Add(ticket);
                    StateChanged?.Invoke();
                    // Готовим
                    await Task.Delay(CookInterval, token);
                    ticket.MarkCooked();
                    lock (CookingTickets) CookingTickets.Remove(ticket);
                    Service.Add(ticket);
                    StateChanged?.Invoke();
                }
            }
            catch (OperationCanceledException) { }
        }
        private async Task ServerWork(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                OrderTicket ticket = await Service.TakeNext(token);

                lock (ServingTickets) ServingTickets.Add(ticket);
                StateChanged?.Invoke();

                ticket.MarkPickedUp();
                await Task.Delay(ServeInterval, token);

                lock (ServingTickets) ServingTickets.Remove(ticket);
                StateChanged?.Invoke();
            }
        }
    }
}