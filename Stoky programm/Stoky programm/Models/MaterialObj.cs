using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stoky_programm.Models
{
    public abstract class MaterialObj
    {
        public enum UnitType
        {
            Piece,
            Kilogram,
            Liter,
            Meter
        }

        private int id;
        private string name;
        private UnitType unit;
        private double pricePerUnit;
        private decimal quantity;
        private DateTime date;
        private bool isActive;
        public int ID
        {
            get
            { return id; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(ID), "ID needs be positive number");
                id = value;
            }
        }
        public string Name
        {
            get
            { return name; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Name is empty", nameof(Name));

                if (value.Length > 100)
                    throw new ArgumentException("Name too large(100)", nameof(Name));
                name = value.Trim();
            }
        }
        public decimal Quantity
        {
            get { return quantity; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(
                        nameof(Quantity), value, "Quantity can not be negative");

                if (unit == UnitType.Piece)
                {
                    if (value % 1 != 0)
                        throw new ArgumentException("Quantity must be an integer", nameof(Quantity));
                    quantity = Math.Round(value, 0);
                }
                else
                {
                    quantity = Math.Round(value, 2);
                }
            }
        }
        public UnitType Unit
        {
            get
            {
                return unit;
            }
            set
            {
                if (!Enum.IsDefined(typeof(UnitType), value))
                    throw new ArgumentException("Unit is incorrect", nameof(Unit));
                unit = value;
            }
        }
        public string UnitString
        {
            get
            {
                return GetUnitDescription(unit);
            }
        }
        public static string GetUnitDescription(UnitType unit)
        {
            switch (unit)
            {
                case UnitType.Piece:  return "шт.";
                case UnitType.Kilogram: return "кг.";
                case UnitType.Meter: return "м.";
                case UnitType.Liter: return "л.";
                default:  return "неизв.";
            }
        }
        public double PricePerUnit
        {
            get
            {
                return pricePerUnit;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(PricePerUnit), "Price cannot be negative");

                if (value > 1000000)
                    throw new ArgumentOutOfRangeException(nameof(PricePerUnit), "Price too large");

                if (double.IsNaN(value) || double.IsInfinity(value))
                    throw new ArgumentException("Price must be a valid number", nameof(PricePerUnit));

                pricePerUnit = Math.Round(value, 2);
            }
        }
        public decimal TotalPrice
        {
            get
            {
                return Quantity * (decimal)PricePerUnit;
            }
        }
        public DateTime Date
        {
            get
            {
                return date;
            }
            set
            {
                if (value == DateTime.MinValue)
                    throw new ArgumentException("Date cannot be minimal", nameof(Date));

                if (value > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(Date), "Date cannot be in the future");
                date = value;
            }
        }

        public void SetDate(DateTime newDate)
        {
            if (!isActive) throw new InvalidOperationException("Not active object");
            var tempDate = date;
            try
            {
                date = newDate;
                if (date == DateTime.MinValue)
                    throw new InvalidOperationException("Date cannot be minimal");
                if (date > DateTime.Now)
                    throw new InvalidOperationException("Date cannot be in the future");
            }
            catch
            {
                date = tempDate;
                throw;
            }
        }

        public void SetPointedDate(DateTime pointedDate)
        {
            if (pointedDate > DateTime.Now)
                throw new ArgumentOutOfRangeException(nameof(pointedDate), "This date cannot be in the future");
            Date = pointedDate;
        }

        public string GetDateInfo()
        {
            if (date.Date == DateTime.Today) return "Сегодня";
            else if (date.Date == DateTime.Today.AddDays(-1)) return "Вчера";
            else return date.ToString("dd.MM.yyyy");
        }
        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }
        //конструктор для сегодняшней даты
        protected MaterialObj(string name, UnitType unit, decimal quantity, double price)
        {
            try
            {
                ID = 0;
                Name = name;
                Unit = unit;
                Quantity = quantity;
                PricePerUnit = price;
                Date = DateTime.Now;
                IsActive = true;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error creating MaterialObj:" + ex.Message);
            }
        }
        protected MaterialObj(string name, UnitType unit, decimal quantity, double price, DateTime date)
        {
            try
            {
                ID = 0;
                Name = name;
                Unit = unit;
                Quantity = quantity;
                PricePerUnit = price;
                Date = date;
                IsActive = true;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error creating MaterialObj:" + ex.Message);
            }
        }

        public void Deactivate()
        {
            if (!isActive)
                throw new InvalidOperationException("Object deactivated");
            isActive = false;
        }
        public void Activate()
        {
            if (isActive)
                throw new InvalidOperationException("Object activated");
            if (id <= 0 || string.IsNullOrEmpty(name))
                throw new InvalidOperationException("Object cannot be activated if the required fields are not filled");

            isActive = true;
        }
        public virtual void Validate()
        {
            var errors = new List<string>();
            if (id < 0)
                errors.Add("ID должен быть положительным числом");
            if (quantity < 0)
                errors.Add("Quantity должено быть рациональным положительным числом");
            if (string.IsNullOrWhiteSpace(name))
                errors.Add("Name должно быть заполнено");
            else if (name.Length > 100)
                errors.Add("Name не может превышать более 100 символов");
            if (!Enum.IsDefined(typeof(UnitType), unit))
                errors.Add("Unit должен быть заполнен");
            if (pricePerUnit < 0)
                errors.Add("Price не может быть отрицательным");
            else if (pricePerUnit > 9999999)
                errors.Add("Price слишком большая");

            if (date == DateTime.MinValue)
                errors.Add("Date должена быть установлена");
            else if (date > DateTime.Now)
                errors.Add("Date не может быть в будущем");

            if (errors.Count > 0)
                throw new InvalidOperationException("Ошибки валидации:" + string.Join("; ", errors));
        }

        public override string ToString()
        {
            return $"\nОбъект:{Name}, кол-во:{Quantity}, цена за {GetUnitDescription(Unit)}:{PricePerUnit:C}\nдата добавления:{GetDateInfo()}, статус={IsActive}";
        }
    }
}