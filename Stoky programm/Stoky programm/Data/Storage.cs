using Stoky_programm.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Stoky_programm.Data
{
    public class Storage
    {
        private List<MaterialObj> allItems;
        private List<Products> products;
        private List<EnterpriseAsset> assets;
        private int nextId;

        public Storage()
        {
            allItems = new List<MaterialObj>();
            products = new List<Products>();
            assets = new List<EnterpriseAsset>();
            nextId = 1;
        }
        private int GetNextId()
        {
            return nextId++;
        }

        public List<MaterialObj> GetAllItems()
        {
            return new List<MaterialObj>(allItems);
        }
        public List<Products> GetAllProducts()
        {
            return new List<Products>(products);
        }
        public List<EnterpriseAsset> GetAllAssets()
        {
            return new List<EnterpriseAsset>(assets);
        }

        public void AddItem(MaterialObj item)
        {
            item.Validate();
            bool itemMerged = false;

            foreach (var obj in allItems)
            {
                if (item.Name == obj.Name && item.Unit == obj.Unit)
                {
                    if (item.GetType() == typeof(Products) && obj.GetType() == typeof(Products))
                    {
                        var existingProduct = (Products)obj;
                        var newProduct = (Products)item;
                        if (existingProduct.Manufacturer == newProduct.Manufacturer && existingProduct.Model == newProduct.Model &&
                            existingProduct.Category == newProduct.Category && Math.Abs(existingProduct.PricePerUnit - newProduct.PricePerUnit) == 0)
                        {
                            existingProduct.Quantity += newProduct.Quantity;
                            itemMerged = true;
                            break;
                        }
                    }
                    else if (item.GetType() == typeof(EnterpriseAsset) && obj.GetType() == typeof(EnterpriseAsset))
                    {
                        var existingAsset = (EnterpriseAsset)obj;
                        var newAsset = (EnterpriseAsset)item;
                        if (existingAsset.InventoryNumber == newAsset.InventoryNumber && existingAsset.Asset == newAsset.Asset &&
                            Math.Abs(existingAsset.PricePerUnit - newAsset.PricePerUnit) == 0)
                        {
                            existingAsset.Quantity += newAsset.Quantity;
                            itemMerged = true;
                            break;
                        }
                    }
                }
            }
            if (!itemMerged)
            {
                item.ID = GetNextId();
                allItems.Add(item);

                if (item.GetType() == typeof(Products))
                {
                    products.Add((Products)item);
                }
                else if (item.GetType() == typeof(EnterpriseAsset))
                {
                    assets.Add((EnterpriseAsset)item);
                }
            }
        }

        public MaterialObj GetItemById(int id)
        {
            foreach (var item in allItems)
            {
                if (item.ID == id)
                {
                    return item;
                }
            }
            return null;
        }

        public void UpdateItemQuantity(int id, decimal quantityChange)
        {
            var item = GetItemById(id);
            if (item != null)
            {
                if (item is Products product)
                {
                    var updatedProduct = new Products(product.Name, product.Unit, product.Quantity + quantityChange,
                        product.PricePerUnit, product.Category, product.Manufacturer, product.Model);
                    DeleteItem(id);
                    AddItem(updatedProduct);
                }
                else if (item is EnterpriseAsset asset)
                {
                    var updatedAsset = new EnterpriseAsset(asset.Name, asset.Unit, asset.Quantity + quantityChange,
                        asset.PricePerUnit, asset.Asset, asset.Condition, asset.InventoryNumber, asset.CommissioningDate);
                    DeleteItem(id);
                    AddItem(updatedAsset);
                }
            }
            else
            {
                throw new ArgumentException($"Объект с ID {id} не найден");
            }
        }
        public void DeleteItem(int id)
        {
            var item = GetItemById(id);
            if (item != null)
            {
                item.Deactivate();

                if (item.GetType() == typeof(Products))
                {
                    products.Remove((Products)item);
                }
                else if (item.GetType() == typeof(EnterpriseAsset))
                {
                    assets.Remove((EnterpriseAsset)item);
                }
                allItems.Remove(item);
            }
            else
            {
                throw new ArgumentException($"Объект с ID {id} не найден");
            }
        }

        public void ActivateItem(int id)
        {
            var item = GetItemById(id);
            if (item != null)
            {
                item.Activate();
                Console.WriteLine($"Объект {item.Name} (ID: {id}) активирован");
            }
            else
            {
                throw new ArgumentException($"Объект с ID {id} не найден");
            }
        }
        public void DeactivateItem(int id)
        {
            var item = GetItemById(id);
            if (item != null)
            {
                item.Deactivate();
                Console.WriteLine($"Объект {item.Name} (ID: {id}) деактивирован");
            }
            else
            {
                throw new ArgumentException($"Объект с ID {id} не найден");
            }
        }
        public List<MaterialObj> Search(string searchTerm)
        {
            var results = new List<MaterialObj>();
            string LowerSearchTerm = searchTerm.ToLower();
            foreach (var item in allItems)
            {
                if (item.Name.ToLower().Contains(LowerSearchTerm) || item.ID.ToString().Contains(LowerSearchTerm))
                {
                    results.Add(item);
                }
            }

            return results;
        }
        public string ExportToText()
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== СКЛАД - ОТЧЕТ ===");
            sb.AppendLine($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}");
            sb.AppendLine($"Всего объектов: {allItems.Count}");

            int activeCount = 0;
            decimal totalValue = 0;

            foreach (var item in allItems)
            {
                if (item.IsActive)
                {
                    activeCount++;
                    totalValue += item.TotalPrice;
                }
            }

            sb.AppendLine($"Активных: {activeCount}");
            sb.AppendLine($"Общая стоимость: {totalValue:C}");
            sb.AppendLine();

            if (products.Count > 0)
            {
                sb.AppendLine("=== ПРОДУКТЫ ===");
                foreach (var product in products)
                {
                    if (product.IsActive)
                    {
                        sb.AppendLine(product.ToString());
                    }
                }
                sb.AppendLine();
            }

            if (assets.Count > 0)
            {
                sb.AppendLine("=== АКТИВЫ ПРЕДПРИЯТИЯ ===");
                foreach (var asset in assets)
                {
                    if (asset.IsActive)
                    {
                        sb.AppendLine(asset.ToString());
                    }
                }
            }

            var needsService = new List<EnterpriseAsset>();
            foreach (var asset in assets)
            {
                if (asset.IsActive && asset.NeedsService())
                {
                    needsService.Add(asset);
                }
            }

            if (needsService.Count > 0)
            {
                sb.AppendLine("\n=== ТРЕБУЕТ ОБСЛУЖИВАНИЯ ===");
                foreach (var asset in needsService)
                {
                    sb.AppendLine($"- {asset.Name} (Инв. №{asset.InventoryNumber})");
                }
            }

            return sb.ToString();
        }
    }
}

