using System.Threading;
using System.Threading.Tasks;

namespace FFS.Models
{
    public enum CustomerState
    {
        InQueue, //в очереди к кассе
        WalkingToCashier, // идёт к кассиру
        AtCashier, // делает заказ
        WaitingForFood,// ждёт заказ
        WaitingForPickup, // ждёт выдачу
        Leaving  //уходит
    }
    public class Customer
    {
        public int Id { get; private set; }
        public CustomerState State { get; set; }
        public OrderTicket Ticket { get; set; }

        public double X { get; set; }
        public double Y { get; set; }

        public double TargetX { get; set; }
        public double TargetY { get; set; }

        //визуальный элемент 
        public object Visual { get; set; }

        public Customer(int id)
        {
            Id = id;
            State = CustomerState.InQueue;
        }
    }
}