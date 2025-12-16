using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace Stoky_programm.Models
{
    [JsonDerivedType(typeof(Products), typeDiscriminator: "product")]
    public class Products : MaterialObj
    {
        public enum ProductCategory
        {
            Other,
            Electronics,
            Furniture,
            Tools,
            Clothing,
            Food,
            Chemicals,
            BuildingMaterials,
            Automotive
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
        //только что произведен и добавлен)
        public Products(string name, UnitType unit, decimal quantity, double price,
            ProductCategory category, string manufacturer, string model)
            : base(name, unit, quantity, price)
        {
            Category = category;
            Manufacturer = manufacturer;
            Model = model;
            ProductionDate = DateTime.Now; 
        }

        // с указанием даты производства
        public Products(string name, UnitType unit, decimal quantity, double price,
            ProductCategory category, string manufacturer, string model, DateTime productionDate)
            : base(name, unit, quantity, price) 
        {
            Category = category;
            Manufacturer = manufacturer;
            Model = model;
            ProductionDate = productionDate; 
        }
        public Products(string name, UnitType unit, decimal quantity, double price,
            DateTime addedDate, ProductCategory category, string manufacturer, string model,
            DateTime productionDate) : base(name, unit, quantity, price, addedDate)
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
            if (ProductionDate > Date)  
                errors.Add("Production date can't be earlier the add to storage Date ");
            if (ProductionDate == DateTime.MinValue)
                errors.Add("Need a production date");
            if (errors.Count > 0)
                throw new InvalidOperationException("Product validation errors: " + string.Join("; ", errors));
        }

        public override string ToString()
        {
            return $"[Продукт]: категория: {GetCategory()}, производитель: {Manufacturer}, наименование: {Model}, {base.ToString()}";
        }

        [JsonConstructor]
        public Products(int id, string name, UnitType unit, decimal quantity, double pricePerUnit,
                DateTime date, bool isActive, ProductCategory category,
                string manufacturer, string model, DateTime productionDate): base(id, name, unit, quantity, pricePerUnit, date, isActive)
        {
            Category = category;
            Manufacturer = manufacturer;
            Model = model;
            ProductionDate = productionDate;
        }
    }
}
