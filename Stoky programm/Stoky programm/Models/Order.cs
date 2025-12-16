using Stoky_programm.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Stoky_programm.Models
{
    public class OrderProduct
    {
        private Products product;
        private decimal orderQuantity;

        public Products Product
        {
            get { return product; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Продукт не может быть null");
                product = value;
            }
        }
        public decimal OrderQuantity
        {
            get { return orderQuantity; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Количество должно быть больше 0");
                orderQuantity = value;
            }
        }
        public decimal TotalPrice
        {
            get { return (decimal)Product.PricePerUnit * OrderQuantity; }
        }
        public override string ToString()
        {
            return $"{Product.Name} - {OrderQuantity} {Product.UnitString} × {Product.PricePerUnit:C} = {TotalPrice:C}";
        }
    }
    public class Order
    {
        [JsonInclude]
        public static int LastOrderNumber { get; set; } = 0;

        private int orderNumber;
        private DateTime orderDate;
        private string customerInfo;
        private OrderStatus status;
        private List<OrderProduct> items;

        public int OrderNumber
        {
            get { return orderNumber; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Номер заказа должен быть положительным");
                orderNumber = value;
            }
        }
        private int GenerateOrderNumber()
        {
            LastOrderNumber++;
            return LastOrderNumber;
        }
        public DateTime OrderDate
        {
            get { return orderDate; }
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentException("Дата создания заказа не может быть в будущем");
                orderDate = value;
            }
        }

        public string CustomerInfo
        {
            get { return customerInfo; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Информация о заказчике не может быть пустой");
                if (value.Length > 400)
                    throw new ArgumentException("Информация клиента слишком длинная");
                customerInfo = value.Trim();
            }
        }
        public OrderStatus Status
        {
            get { return status; }
            set
            {
                if (!Enum.IsDefined(typeof(OrderStatus), value))
                    throw new ArgumentException("Неверный статус заказа");
                status = value;
            }
        }
        public List<OrderProduct> Items
        {
            get { return items; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Список товаров не может быть null");
                items = value;
            }
        }
        public decimal TotalAmount
        {
            get
            {
                decimal total = 0;
                foreach (var item in items)
                {
                    total += item.TotalPrice;
                }
                return total;
            }
        }

        public Order()
        {
            items = new List<OrderProduct>();
            orderDate = DateTime.Now;
            status = OrderStatus.New;
            orderNumber = GenerateOrderNumber();
        }

        public Order(string customerInfo) : this()
        {
            CustomerInfo = customerInfo;
        }

        public void AddProductToOrder(Products product, decimal quantity)
        {
            if (product == null)
                throw new ArgumentNullException("Продукт не может быть null");

            if (quantity <= 0)
                throw new ArgumentException("Количество должно быть больше 0");
            

            if (!product.IsActive)
                throw new InvalidOperationException("Продукт не активен");
            if (quantity > product.Quantity)
                throw new InvalidOperationException( $"Недостаточно товара {product.Name} Запрашивается: {quantity}, доступно: {product.Quantity}");

            var existingItem = items.FirstOrDefault(i => i.Product.ID == product.ID);
            if (existingItem != null)
            {
                decimal newTotalQuantity = existingItem.OrderQuantity + quantity;
                if (newTotalQuantity > product.Quantity)
                    throw new InvalidOperationException($"Недостаточно товара {product.Name}. \nУже в заказе: {existingItem.OrderQuantity}, добавляется: {quantity}, " +
                        $"всего будет: {newTotalQuantity}, доступно: {product.Quantity}");
                existingItem.OrderQuantity = newTotalQuantity;
            }
            else
            {
                var orderProduct = new OrderProduct
                {
                    Product = product,
                    OrderQuantity = quantity
                };
                items.Add(orderProduct);
            }
        }

        public void RemoveProduct(int productId)
        {
            var item = items.FirstOrDefault(i => i.Product.ID == productId);
            if (item != null)
            {
                if (Status == OrderStatus.New || Status == OrderStatus.Confirmed)
                {
                    item.Product.Quantity += item.OrderQuantity;
                }
                items.Remove(item);
            }
        }
        public bool ConfirmOrder(Storage storage)
        {
            if (items.Count == 0)
                throw new InvalidOperationException("Заказ пустой");
            if (status != OrderStatus.New)
                throw new InvalidOperationException("Можно подтверждать только новые заказы");

            foreach (var item in items)
            {
                var productInStorage = storage.GetItemById(item.Product.ID) as Products;
                if (productInStorage == null)
                    throw new InvalidOperationException($"Товар '{item.Product.Name}' не найден на складе");
                if (productInStorage.Quantity < item.OrderQuantity)
                    throw new InvalidOperationException($"Недостаточно '{item.Product.Name}'. На складе: {productInStorage.Quantity}, нужно: {item.OrderQuantity}");
            }
            foreach (var item in items)
            {
                var productInStorage = storage.GetItemById(item.Product.ID) as Products;
                productInStorage.Quantity -= item.OrderQuantity;
                if (productInStorage.Quantity == 0)
                {
                    productInStorage.IsActive = false;
                }
            }

            status = OrderStatus.Confirmed;
            return true;
        }

        public bool CancelOrder(Storage storage)
        {
            if (status == OrderStatus.Completed || status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Нельзя отменить завершенный или отмененный заказ");

            if (status == OrderStatus.Confirmed)
            {
                foreach (var item in items)
                {
                    var productInStorage = storage.GetItemById(item.Product.ID) as Products;
                    if (productInStorage != null)
                    {
                        productInStorage.Quantity += item.OrderQuantity;
                        if (!productInStorage.IsActive && productInStorage.Quantity > 0)
                        {
                            productInStorage.IsActive = true;
                        }
                    }
                }
            }
            status = OrderStatus.Cancelled;
            return true;
        }

        public bool CompleteOrder()
        {
            if (status != OrderStatus.Confirmed)
                throw new InvalidOperationException("Можно завершать только подтвержденные заказы");
            status = OrderStatus.Completed;
            return true;
        }

        public string GetStatusText()
        {
            switch (status)
            {
                case OrderStatus.New: return "Новый";
                case OrderStatus.Confirmed: return "Подтвержден";
                case OrderStatus.Completed: return "Завершен";
                case OrderStatus.Cancelled: return "Отменен";
                default: return "Неизвестно";
            }
        }
        public string StatusDisplay => GetStatusText();
        public int TotalItemsCount
        {
            get
            {
                int total = 0;
                foreach (var item in Items)
                {
                    total += (int)item.OrderQuantity;
                }
                return total;
            }
        }

        public string GetDetailedInfo()
        {
            var info = $"Заказ №{OrderNumber}\nДата: {OrderDate:dd.MM.yyyy HH:mm}\nКлиент: {CustomerInfo}\n"
                +$"Статус: {GetStatusText()}\nТоваров: {items.Count}\nОбщая сумма: {TotalAmount:C}\nСостав заказа:\n";

            foreach (var item in items)
            {
                info += $" • {item}\n";
            }

            return info;
        }

        public override string ToString()
        {
            return $"Заказ №{OrderNumber:00000} от {OrderDate:dd.MM.yyyy} - {TotalAmount:C} ({GetStatusText()})";
        }

        public enum OrderStatus
        {
            New,
            Confirmed,
            Completed,
            Cancelled
        }
    }

}