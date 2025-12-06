using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stoky_programm.Models
{
    public class EnterpriseAsset : MaterialObj
    {
        public enum AssetType
        {
            Equipment,
            Vehicle,
            Tools,
            Furniture,
            Other
        }
        public enum Conditions
        { 
            New,
            Good,
            Satisfactory,
            Damaged,
            Broken,
            Obsolete
        }
        private AssetType asset;
        private Conditions condition;
        private string inventoryNumber;
        private DateTime commissioningDate;
        private DateTime? lastService;

        public AssetType Asset
        {
            get
            {
                return asset;
            }
            set
            {
                if (!Enum.IsDefined(typeof(AssetType), value))
                    throw new ArgumentException("Invalid product Type", nameof(AssetType));
                asset = value;
            }
        }

        public Conditions Condition
        {
            get
            {
                return condition;
            }
            set
            {
                if (!Enum.IsDefined(typeof(Conditions), value))
                    throw new ArgumentException("Invalid product condition", nameof(Conditions));
                condition = value;
            }
        }
        public string InventoryNumber
        {
            get
            {
                return inventoryNumber;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (value.Length > 10)
                        throw new ArgumentException("Inventory number too long", nameof(InventoryNumber));
                    foreach (char c in value)
                    {
                        if (!char.IsDigit(c))
                            throw new ArgumentException("Inventory number can contain only digits", nameof(InventoryNumber));
                    }
                }
                inventoryNumber = value.Trim();
            }
        }

        public DateTime CommissioningDate
        {
            get
            {
                return commissioningDate;
            }
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentOutOfRangeException(nameof(CommissioningDate), "commisioning Date can not be in the future");
                commissioningDate = value;
            }
        }
        public DateTime? LastService
        {
            get { return lastService; }
            set
            {
                if (value == null)
                {
                    lastService = value;
                    return;
                }
                if (value > DateTime.Now || value<CommissioningDate)
                    throw new ArgumentOutOfRangeException(nameof(LastService), "LastService date can not be in the future or in the past");
                lastService = value;
            }
        }

        //для сегодняшнего
        public EnterpriseAsset(string name, UnitType unit,decimal quantity, double price, 
            AssetType asset, Conditions condition, string inventoryNumber,DateTime commissioningDate)
            : base(name, unit, quantity, price)
        {
            Unit = UnitType.Piece;
            Asset =asset;
            Condition=condition;
            InventoryNumber=inventoryNumber;
            CommissioningDate=commissioningDate;
            LastService= null;
        }
        public EnterpriseAsset(string name,  UnitType unit, decimal quantity, double price, DateTime date,
            AssetType asset, Conditions condition, string inventoryNumber, DateTime commissioningDate)
            : base(name, unit, quantity, price, date)
        {
            Unit=UnitType.Piece;
            Asset = asset;
            Condition = condition;
            InventoryNumber = inventoryNumber;
            CommissioningDate = commissioningDate;
            LastService = null;
        }
        public string GetAssetType()
        {
            switch (asset)
            {
                case AssetType.Equipment: return "Оборудование";
                case AssetType.Vehicle:return "Транспорт";
                case AssetType.Tools:   return "Инструменты";
                case AssetType.Furniture: return "Мебель";
                case AssetType.Other: return "Другое";
                default: return "Неизвестный тип";
            }
        }
        public string GetCondition()
        {
            switch (condition)
            {
                case Conditions.New: return "Новое";
                case Conditions.Good: return "Хорошее";
                case Conditions.Satisfactory: return "Удовлетворительное";
                case Conditions.Damaged: return "Повреждено";
                case Conditions.Broken: return "Сломано";
                case Conditions.Obsolete: return "Устарело";
                default: return "Неизвестное состояние";
            }
        }
        public void Damaged()
        {
            if (condition < Conditions.Damaged) condition = Conditions.Damaged;
        }

        public void Repair()
        {
            if (condition == Conditions.Damaged || condition == Conditions.Broken) condition = Conditions.Satisfactory;
            lastService = DateTime.Now;
        }

        public bool NeedsService()
        {
            int serviceInterval;
            switch (asset)
            {
                case AssetType.Vehicle:
                    serviceInterval = 6;
                    break;
                case AssetType.Equipment:
                    serviceInterval = 12;
                    break;
                case AssetType.Tools:
                    serviceInterval = 3;
                    break;
                default:
                    serviceInterval = 12;
                    break;
            }
            if (LastService == null)
            {
                return CommissioningDate < DateTime.Now.AddMonths(-serviceInterval);
            }
            DateTime nextService = lastService.Value.AddMonths(serviceInterval);
            return DateTime.Now >= nextService;
        }
        public override void Validate()
        {
            base.Validate();
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(inventoryNumber))
                errors.Add("need inventory number");
            if (CommissioningDate > DateTime.Now)
                errors.Add("Commisioning date cannot be in the future");
            if (LastService.HasValue && LastService.Value < CommissioningDate)
                errors.Add("Last service date can not earlyer than commisioning date");
            if (errors.Count > 0)
                throw new InvalidOperationException("Product validation errors: " + string.Join("; ", errors));
        }

        public override string ToString()
        {
            string needsService = NeedsService() ? "(Требует обслуживания)" : "";
            return $"[Объект предприятия]: {base.ToString()}, тип: {GetAssetType()}, состояние: {GetCondition()} {needsService}, инв.номер: {InventoryNumber}, дата ввода: {CommissioningDate:dd.MM.yyyy}";
        }
    }
}
