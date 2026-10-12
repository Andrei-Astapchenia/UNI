using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FFS.Models
{
    public class OrderTicket
    {
        private static int lastNumber = 0;

        public int Number { get; private set; }
        public TaskCompletionSource<bool> CookingDone { get; private set; } //промис для официанта
        public TaskCompletionSource<bool> PickupDone { get; private set; } //промис для клиента

        public OrderTicket()
        {
            lastNumber++;
            Number = lastNumber;

            CookingDone = new TaskCompletionSource<bool>();
            PickupDone = new TaskCompletionSource<bool>();
        }
        public void MarkCooked()
        {
            CookingDone.TrySetResult(true); //повар выполнил работу
        }
        public void MarkPickedUp()
        {
            PickupDone.TrySetResult(true);//официант выполнил работу
        }
        public void Cancel()
        {
            CookingDone.TrySetCanceled();
            PickupDone.TrySetCanceled();
        }

        public override string ToString()
        {
            return "#" + Number;
        }
    }
}