using Stoky_programm.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Stoky_programm.Data
{
    public class Storage
    {
        private JsonData data;
        private List<MaterialObj> allItems => data.AllItems;
        private List<Order> orders => data.Orders;

        private string dataFilePath = "storage_data.json";
        public Storage()
        {
            dataFilePath = "storage_data.json";
            Console.WriteLine($"Путь сохранения: {Path.GetFullPath(dataFilePath)}");
            Debug.WriteLine($"Путь сохранения: {Path.GetFullPath(dataFilePath)}");
            LoadFromFile();
        }

        public Storage(string filePath)
        {
            dataFilePath = filePath;
            LoadFromFile();
        }
        private int GetNextId()
        {
            int id = data.NextId;
            data.NextId++;  
            SaveToFile();
            return id;
        }

        // сохранен JSON
        public void SaveToFile()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                string json = JsonSerializer.Serialize(data, options);
                File.WriteAllText(dataFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        // загрузка JSON
        public void LoadFromFile()
        {
            try
            {
                if (File.Exists(dataFilePath))
                {
                    string json = File.ReadAllText(dataFilePath);
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() },
                        IncludeFields = true,
                        PropertyNameCaseInsensitive = true
                    };

                    data = JsonSerializer.Deserialize<JsonData>(json, options);
                    RenumberItems();
                }
                else
                {
                    data = new JsonData
                    {
                        FilePath = dataFilePath
                    };
                    SaveToFile();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки: {ex.Message}");
                data = new JsonData
                {
                    FilePath = dataFilePath
                };
            }
        }


        public List<MaterialObj> GetAllItems()
        {
            return new List<MaterialObj>(allItems);
        }

        public List<Products> GetAllProducts()
        {
            return allItems.OfType<Products>().ToList();
        }
        public List<EnterpriseAsset> GetAllAssets()
        {
            return allItems.OfType<EnterpriseAsset>().ToList();
        }
        public void AddItem(MaterialObj item)
        {
            item.Validate();
            if (item is EnterpriseAsset addToAdd)
            {
                var existingAsset = allItems.OfType<EnterpriseAsset>().FirstOrDefault(a => a.InventoryNumber == addToAdd.InventoryNumber);
                if (existingAsset != null && existingAsset.ID != addToAdd.ID)
                {
                    throw new InvalidOperationException(
                        $"Актив с инвентарным номером {addToAdd.InventoryNumber} уже существует");
                }
            }
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
            }
            SaveToFile();
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
                        product.PricePerUnit, product.Date, product.Category, product.Manufacturer, product.Model, product.ProductionDate);
                    DeleteItem(id);
                    AddItem(updatedProduct);
                }
                else if (item is EnterpriseAsset asset)
                {
                    var updatedAsset = new EnterpriseAsset(asset.Name, asset.Unit, asset.Quantity + quantityChange,
                        asset.PricePerUnit, asset.Date, asset.Asset, asset.Condition, asset.InventoryNumber, asset.CommissioningDate);
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
                if (item is Products product)
                {
                    var ordersWithProduct = orders.Where(o =>o.Status != Order.OrderStatus.Cancelled &&
                        o.Items.Any(i => i.Product.ID == id)).ToList();
                    if (ordersWithProduct.Any())
                    {
                        throw new InvalidOperationException(
                            $"Продукт '{item.Name}' используется в заказах №" +
                            $"{string.Join(", ", ordersWithProduct.Select(o => o.OrderNumber))}. ");
                    }
                }
                item.Deactivate();
                allItems.Remove(item);
                RenumberItems();

                SaveToFile();
            }
            else
            {
                throw new ArgumentException($"Объект с ID {id} не найден");
            }
        }
        private void RenumberItems()
        {
            var sortedItems = allItems.OrderBy(item => item.ID).ToList();
            int newId = 1;
            foreach (var item in sortedItems)
            {
                item.ID = newId++;
            }
            data.NextId = newId;
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
        public void AddOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("Заказ не может быть null");

            orders.Add(order);
            SaveToFile();
        }
        public bool DeleteOrder(int orderNumber)
        {
            var order = orders.FirstOrDefault(o => o.OrderNumber == orderNumber);
            if (order != null && order.Status == Order.OrderStatus.Cancelled)
            {
                bool result = orders.Remove(order);
                if (result) SaveToFile();
                return result;
            }
            return false;
        }

        public List<Order> GetAllOrders()
        {
            return new List<Order>(orders);
        }
        public List<Order> GetOrdersByStatus(Order.OrderStatus status)
        {
            return orders.Where(o => o.Status == status).ToList();
        }
        public Order GetOrderByNumber(int orderNumber)
        {
            return orders.FirstOrDefault(o => o.OrderNumber == orderNumber);
        }


        public string ExportToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };

            return JsonSerializer.Serialize(data, options);
        }

        // Метод для импорта данных
        public void ImportFromJson(string json)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };

                data = JsonSerializer.Deserialize<JsonData>(json, options);
                SaveToFile();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка импорта: {ex.Message}");
            }
        }
    }
}