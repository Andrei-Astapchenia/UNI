using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stoky_programm.Models
{
    public class Products : MaterialObj
    {
        public enum ProductCategory
        {
            Electronics,
            Furniture,
            Tools,
            Clothing,
            Food,
            Chemicals,
            BuildingMaterials,
            Automotive,
            Other
        }
        private ProductCategory category;
        private string manufacturer;
        private DateTime productionDate;
        private string model;//marker name

        public ProductCategory Category
        {
            get
            {
                return category;
            }
            set
            {
                if (!Enum.IsDefined(typeof(ProductCategory), value))
                    throw new ArgumentException("Invalid product category", nameof(Category));
                category = value;
            }
        }

        public string Manufacturer
        {
            get
            {
                return manufacturer;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (value.Length > 100)
                        throw new ArgumentException("Manufacturer name too long", nameof(Manufacturer));
                }
                manufacturer = value.Trim();
            }
        }

        public DateTime ProductionDate
        {
            get
            {
                return productionDate;
            }
            set
            {
                if (value == DateTime.MinValue)
                    throw new ArgumentException("Production date cannot be minimal", nameof(ProductionDate));
                if (value > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(ProductionDate), "Production date cannot be in the future");
                productionDate = value;
            }
        }
        public string Model
        {
            get { return model; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && value.Length > 50)
                    throw new ArgumentException("Model name too long", nameof(Model));
                model = value.Trim();
            }
        }

        //для сегодняшнего
        public Products(string name, UnitType unit, decimal quantity, double price, ProductCategory category,
            string manufacturer = null, string model = null): base( name, unit, quantity, price)
        {
            Category = category;
            Manufacturer = manufacturer;
            Model = model;
            ProductionDate = DateTime.Now;
        }

        public Products(string name,UnitType unit, decimal quantity, double price, DateTime date,
            ProductCategory category, string manufacturer, string model,DateTime productionDate): base(name,unit, quantity, price, date)
        {
            Category = category;
            Manufacturer = manufacturer;
            Model = model;
            ProductionDate = productionDate;
        }
        public string GetCategory()
        {
            switch (category)
            {
                case ProductCategory.Electronics: return "Электроника";
                case ProductCategory.Furniture: return "Мебель";
                case ProductCategory.Tools: return "Инструменты";
                case ProductCategory.Clothing: return "Одежда";
                case ProductCategory.Food: return "Продукты питания";
                case ProductCategory.Chemicals: return "Химикаты";
                case ProductCategory.BuildingMaterials: return "Строительные материалы";
                case ProductCategory.Automotive: return "Автозапчасти";
                case ProductCategory.Other: return "Другое";
                default: return "Неизвестная категория";
            }
        }

        public override void Validate()
        {
            base.Validate();

            var errors = new List<string>();

            if (ProductionDate > DateTime.Now)
                errors.Add("Production date cannot be in the future");

            if (errors.Count > 0)
                throw new InvalidOperationException("Product validation errors: " + string.Join("; ", errors));
        }

        public override string ToString()
        {
            return $"[Продукт]: категория: {GetCategory()}, производитель: {Manufacturer}, наименование: {Model}, {base.ToString()}";
        }
    }
}
