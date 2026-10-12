using System;
using System.Collections.Generic;
using System.Text;

namespace FFS.Models
{
    public class Cashier
    {
        public int Id { get; set; }
        public Customer CurrentCustomer { get; set; }
        public OrderTicket CurrentTicket { get; set; }
        public bool IsBusy => CurrentCustomer != null;
    }
}
