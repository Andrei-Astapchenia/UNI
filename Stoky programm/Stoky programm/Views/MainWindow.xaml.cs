using Stoky_programm.Data;
using Stoky_programm.Models;
using Stoky_programm.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace Stoky_programm.Views
{
    public partial class MainWindow : Window
    {
        private Storage storage;
        private Order currentOrder;
        private List<MaterialObj> allItemsCache;


        public MainWindow()
        {
            InitializeComponent();
            storage = new Storage();
            InitializeCurrentOrder();
            //AddTestData();
            LoadAllItems();
            this.Closing += MainWindow_Closing;
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                storage.SaveToFile();
                Console.WriteLine("Данные сохранены");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void InitializeCurrentOrder()
        {
            currentOrder = new Order();
        }

        private void AddTestData()
        {
            if (storage.GetAllItems().Count > 0 || storage.GetAllOrders().Count > 0)
            {
                return;
            }
            try
            {
                var product1 = new Products(
    "Ноутбук Dell",
    MaterialObj.UnitType.Piece,
    10,
    50000,
    DateTime.Now.AddMonths(-1), // Дата добавления (месяц назад)
    Products.ProductCategory.Electronics,
    "Dell",
    "XPS 15",
    DateTime.Now.AddMonths(-2) // Дата производства (2 месяца назад)
);

                var product2 = new Products(
                    "Офисный стул",
                    MaterialObj.UnitType.Piece,
                    15,
                    12000,
                    DateTime.Now.AddMonths(-3),
                    Products.ProductCategory.Furniture,
                    "IKEA",
                    "MARKUS",
                    DateTime.Now.AddMonths(-4)
                );

                var product3 = new Products(
                    "Молоток",
                    MaterialObj.UnitType.Piece,
                    25,
                    500,
                    DateTime.Now.AddMonths(-2),
                    Products.ProductCategory.Tools,
                    "Stanley",
                    "FatMax",
                    DateTime.Now.AddMonths(-3)
                );

                // Тестовые активы
                var asset1 = new EnterpriseAsset(
                    "Грузовой автомобиль",
                    MaterialObj.UnitType.Piece,
                    1,
                    150000,
                    EnterpriseAsset.AssetType.Vehicle,
                    EnterpriseAsset.Conditions.Good,
                    123,
                    DateTime.Now.AddYears(-1)
                );

                var asset2 = new EnterpriseAsset(
                    "Токарный станок",
                    MaterialObj.UnitType.Piece,
                    1,
                    80000,
                    EnterpriseAsset.AssetType.Equipment,
                    EnterpriseAsset.Conditions.Satisfactory,
                    991,
                    DateTime.Now.AddMonths(-18)
                );

                var asset3 = new EnterpriseAsset(
                    "Шлифовальная машина",
                    MaterialObj.UnitType.Piece,
                    1,
                    35000,
                    EnterpriseAsset.AssetType.Tools,
                    EnterpriseAsset.Conditions.Broken,
                    1,
                    DateTime.Now.AddMonths(-6)
                );

                // Добавляем в хранилище
                storage.AddItem(product1);
                storage.AddItem(product2);
                storage.AddItem(product3);
                storage.AddItem(asset1);
                storage.AddItem(asset2);
                storage.AddItem(asset3);

                // Тестовый заказ
                var order = new Order("ООО 'ТехноПром', тел: 8-900-123-45-67");
                order.AddProductToOrder(product1, 2);
                order.AddProductToOrder(product3, 5);
                order.ConfirmOrder(storage);
                storage.AddOrder(order);

                MessageBox.Show("Тестовые данные загружены успешно!", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки тестовых данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Загрузка 
        private void LoadAllItems()
        {
            Items.ItemsSource = storage.GetAllItems();
            allItemsCache = storage.GetAllItems();
            UpdateStatusBar($"Всего объектов: {storage.GetAllItems().Count}");
        }
        private void LoadProducts()
        {
            Items.ItemsSource = storage.GetAllProducts();
            UpdateStatusBar($"Продуктов: {storage.GetAllProducts().Count}");
        }
        private void LoadAssets()
        {
            var allAssets = storage.GetAllAssets();
            Assets.ItemsSource = allAssets;
            Items.ItemsSource = allAssets;
            UpdateStatusBar($"Всего активов: {allAssets.Count}. Сломанных: {allAssets.Count(a => a.Condition == EnterpriseAsset.Conditions.Broken)}, " +
                   $"Поврежденных: {allAssets.Count(a => a.Condition == EnterpriseAsset.Conditions.Damaged)}, " +
                   $"Требуют обслуживания: {allAssets.Count(a => a.NeedsService())}");
        }
        private void LoadOrders()
        {
            Orders.ItemsSource = storage.GetAllOrders();
            UpdateStatusBar($"Всего заказов: {storage.GetAllOrders().Count}");
        }
        private void UpdateStatusBar(string message)
        {
            TableStatus.Text = message;
        }
        //управление панелями
        private void ShowPanel(Grid panelToShow)
        {
            AllObj.Visibility = Visibility.Collapsed;
            CreateOrder.Visibility = Visibility.Collapsed;
            AllOrders.Visibility = Visibility.Collapsed;
            AllAssets.Visibility = Visibility.Collapsed;

            PanelAllObjButtons.Visibility = Visibility.Collapsed;
            PanelCreateOrderButtons.Visibility = Visibility.Collapsed;
            PanelAllAssetsButtons.Visibility = Visibility.Collapsed;
            PanelAllOrdersButtons.Visibility = Visibility.Collapsed;

            panelToShow.Visibility = Visibility.Visible;
            if (panelToShow == AllObj)
            {
                PanelAllObjButtons.Visibility = Visibility.Visible;
                LoadAllItems();
            }
            else if (panelToShow == CreateOrder)
            {
                PanelCreateOrderButtons.Visibility = Visibility.Visible;
                InitializeCreateOrderTable();
            }
            else if (panelToShow == AllOrders)
            {
                PanelAllOrdersButtons.Visibility = Visibility.Visible;
                LoadOrders();
            }
            else if (panelToShow == AllAssets)
            {
                PanelAllAssetsButtons.Visibility = Visibility.Visible;
                LoadAssets();
            }
        }

        //Меню навигации
        private void ButtonInventory_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(AllObj);
        }
        private void ButtonCreateOrder_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(CreateOrder);
        }
        private void ButtonOrders_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(AllOrders);
        }
        private void ButtonAssets_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel(AllAssets);
            LoadAssets();
        }
        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }



        //Верхняя панель 
        private void ButtonShowAll_Click(object sender, RoutedEventArgs e)
        {
            LoadAllItems();
        }
        private void ButtonShowProducts_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }
        private void ButtonAddProduct_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new WindowAddProduct();
            if (addWindow.ShowDialog() == true && addWindow.NewProduct != null)
            {
                try
                {
                    storage.AddItem(addWindow.NewProduct);
                    LoadProducts();
                    MessageBox.Show("Продукт успешно добавлен", "Успех");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
                }
            }
        }
        private void ButtonShowAssets_Click(object sender, RoutedEventArgs e)
        {
            LoadAssets();
        }
        private void ButtonAddAsset_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new WindowAddAsset();
            if (addWindow.ShowDialog() == true && addWindow.NewAsset != null)
            {
                try
                {
                    storage.AddItem(addWindow.NewAsset);
                    LoadAssets();
                    MessageBox.Show("Актив успешно добавлен", "Успех");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
                }
            }
        }
        //кнопки внизу
        private void ButtonDetails_Click(object sender, RoutedEventArgs e)
        {
            if (Items.SelectedItem is Products product)
            {
                MessageBox.Show(product.ToString(), $"Детали: {product.Name}");
            }
            else if (Items.SelectedItem is EnterpriseAsset asset)
            {
                MessageBox.Show(asset.ToString(), $"Детали: {asset.Name}");
            }
            else
            {
                MessageBox.Show("Выберите объект для просмотра деталей", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (Items.SelectedItem is MaterialObj item)
            {
                var result = MessageBox.Show($"Удалить '{item.Name}'?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        storage.DeleteItem(item.ID);
                        LoadAllItems();
                        MessageBox.Show("Объект удален", "Успех");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите объект для удаления", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        //Для главного окна
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                Items.ItemsSource = allItemsCache;
                UpdateStatusBar($"Всего объектов: {allItemsCache?.Count ?? 0}");
                return;
            }
            string searchTerm = SearchTextBox.Text.ToLower();
            var filteredItems = new List<MaterialObj>();

            foreach (var item in allItemsCache)
            {
                if (item.ID.ToString().Contains(searchTerm))
                {
                    filteredItems.Add(item);
                    continue;
                }
                if (item.Name.ToLower().Contains(searchTerm))
                {
                    filteredItems.Add(item);
                    continue;
                }
                if (item is Products product)
                {
                    if (product.Manufacturer?.ToLower().Contains(searchTerm) == true)
                    {
                        filteredItems.Add(item);
                        continue;
                    }
                    if (product.Model?.ToLower().Contains(searchTerm) == true)
                    {
                        filteredItems.Add(item);
                        continue;
                    }
                }
                if (item is EnterpriseAsset asset)
                {
                    if (asset.InventoryNumber.ToString().Contains(searchTerm))
                    {
                        filteredItems.Add(item);
                        continue;
                    }
                }
            }
            Items.ItemsSource = filteredItems;
            UpdateStatusBar($"Найдено объектов: {filteredItems.Count} из {allItemsCache.Count}");
        }
        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            Items.ItemsSource = allItemsCache;
            UpdateStatusBar($"Всего объектов: {allItemsCache?.Count ?? 0}");
        }
        private void Items_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Items.SelectedItem is Products selectedProduct)
            {
                EditProduct(selectedProduct);
            }
            else if (Items.SelectedItem is EnterpriseAsset selectedAsset)
            {
                EditAsset(selectedAsset);
            }
        }

        private void EditProduct(Products product)
        {
            var editWindow = new WindowAddProduct();
            editWindow.txtName.Text = product.Name;
            editWindow.txtManufacturer.Text = product.Manufacturer;
            editWindow.txtModel.Text = product.Model;
            editWindow.cmbCategory.SelectedItem = product.Category;
            editWindow.txtQuantity.Text = product.Quantity.ToString();
            editWindow.txtPrice.Text = product.PricePerUnit.ToString();
            editWindow.ProductionDate.SelectedDate = product.ProductionDate;
            editWindow.AddedDate.SelectedDate = product.Date;
            editWindow.Title = "Редактирование продукта";
            if (editWindow.ShowDialog() == true && editWindow.NewProduct != null)
            {
                try
                {
                    storage.DeleteItem(product.ID);
                    storage.AddItem(editWindow.NewProduct);
                    LoadAllItems();
                    MessageBox.Show("Продукт успешно обновлен", "Успех");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка");
                }
            }
        }

        private void EditAsset(EnterpriseAsset asset)
        {
            var editWindow = new WindowAddAsset();
            editWindow.txtName.Text = asset.Name;
            editWindow.cmbAssetType.SelectedItem = asset.Asset;
            editWindow.cmbCondition.SelectedItem = asset.Condition;
            editWindow.txtInventoryNumber.Text = asset.InventoryNumber.ToString();
            editWindow.txtPrice.Text = asset.PricePerUnit.ToString();
            editWindow.dpCommissioningDate.SelectedDate = asset.CommissioningDate;
            editWindow.Title = "Редактирование актива";
            if (editWindow.ShowDialog() == true && editWindow.NewAsset != null)
            {
                try
                {
                    if (asset.InventoryNumber != editWindow.NewAsset.InventoryNumber)
                    {
                        var existingAsset = storage.GetAllAssets().FirstOrDefault(a => a.InventoryNumber == editWindow.NewAsset.InventoryNumber);
                        if (existingAsset != null)
                        {
                            MessageBox.Show($"Актив с инвентарным номером {editWindow.NewAsset.InventoryNumber} уже существует", "Ошибка");
                            return;
                        }
                    }
                    storage.DeleteItem(asset.ID);
                    storage.AddItem(editWindow.NewAsset);
                    LoadAllItems();
                    MessageBox.Show("Актив успешно обновлен", "Успех");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка");
                }
            }
        }


        //панель создания заказа
        private void InitializeCreateOrderTable()
        {
            var availableProducts = storage.GetAllProducts().Where(p => p.IsActive && p.Quantity > 0).ToList();

            AvailableProducts.ItemsSource = availableProducts;
            InitializeCurrentOrder();
            OrderedItems.ItemsSource = currentOrder.Items;
            UpdateOrderTotalPrice();

            TextCustomerInfo.Text = "";
            TextCustomerInfo.Focus();
        }
        private void ButtonAddProductToOrder_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableProducts.SelectedItem is Products selectedProduct)
            {
                var quantityWindow = new QuantityInputWindow(selectedProduct.Name, selectedProduct.Quantity);
                if (quantityWindow.ShowDialog() == true)
                {
                    try
                    {
                        currentOrder.AddProductToOrder(selectedProduct, quantityWindow.SelectedQuantity);
                        OrderedItems.ItemsSource = null;
                        OrderedItems.ItemsSource = currentOrder.Items;
                        UpdateOrderTotalPrice();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для добавления", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void ButtonDeleteProductFromOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is OrderProduct orderItem)
            {
                try
                {
                    currentOrder.RemoveProduct(orderItem.Product.ID);
                    OrderedItems.ItemsSource = null;
                    OrderedItems.ItemsSource = currentOrder.Items;
                    UpdateOrderTotalPrice();

                    var availableProducts = storage.GetAllProducts().Where(p => p.IsActive && p.Quantity > 0).ToList();
                    AvailableProducts.ItemsSource = availableProducts;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления товара: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для удаления из заказа",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void ButtonConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextCustomerInfo.Text))
            {
                MessageBox.Show("Введите информацию о клиенте", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TextCustomerInfo.Focus();
                return;
            }

            if (currentOrder.Items.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один товар в заказ", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                currentOrder.CustomerInfo = TextCustomerInfo.Text;
                currentOrder.ConfirmOrder(storage);
                storage.AddOrder(currentOrder);

                MessageBox.Show($"Заказ №{currentOrder.OrderNumber} подтвержден успешно!\nСумма: {currentOrder.TotalAmount:F2} руб.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                InitializeCurrentOrder();
                ShowPanel(AllOrders);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подтверждения заказа: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ButtonClearOrder_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Очистить текущий заказ?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                InitializeCurrentOrder();
                OrderedItems.ItemsSource = currentOrder.Items;
                UpdateOrderTotalPrice();
            }
        }
        private void UpdateOrderTotalPrice()
        {
            TotalOrderText.Text = $"{currentOrder.TotalAmount:F2} руб.";
        }



        //панель заказов
        private void ButtonCancelSelectedOrder_Click(object sender, RoutedEventArgs e)
        {
            if (Orders.SelectedItem is Order selectedOrder)
            {
                var result = MessageBox.Show($"Отменить заказ №{selectedOrder.OrderNumber}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        selectedOrder.CancelOrder(storage);
                        LoadOrders();
                        LoadAllItems();
                        MessageBox.Show("Заказ отменен", "Успех");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка отмены заказа: {ex.Message}", "Ошибка");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для отмены", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void ButtonCompleteSelectedOrder_Click(object sender, RoutedEventArgs e)
        {
            if (Orders.SelectedItem is Order selectedOrder)
            {
                var result = MessageBox.Show($"Завершить заказ №{selectedOrder.OrderNumber}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        selectedOrder.CompleteOrder();
                        LoadOrders();
                        MessageBox.Show("Заказ завершен", "Успех");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка завершения заказа: {ex.Message}", "Ошибка");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для завершения", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void ButtonViewOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            if (Orders.SelectedItem is Order selectedOrder)
            {
                string orderDetails = $"Заказ №{selectedOrder.OrderNumber}\n";
                orderDetails += $"Дата: {selectedOrder.OrderDate:dd.MM.yyyy HH:mm}\n";
                orderDetails += $"Клиент: {selectedOrder.CustomerInfo}\n";
                orderDetails += $"Статус: {selectedOrder.GetStatusText()}\n";
                orderDetails += $"Товаров: {selectedOrder.Items.Count}\n";
                orderDetails += $"Общая сумма: {selectedOrder.TotalAmount:C}\n\n";
                orderDetails += "Состав заказа:\n";

                foreach (var item in selectedOrder.Items)
                {
                    orderDetails += $"  • {item}\n";
                }

                MessageBox.Show(orderDetails, $"Детали заказа №{selectedOrder.OrderNumber}");
            }
            else
            {
                MessageBox.Show("Выберите заказ для просмотра", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void ButtonDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (Orders.SelectedItem is Order selectedOrder)
            {
                if (selectedOrder.Status != Order.OrderStatus.Cancelled)
                {
                    MessageBox.Show("Можно удалять только отмененные заказы!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var result = MessageBox.Show($"Удалить заказ №{selectedOrder.OrderNumber}?","Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        storage.DeleteOrder(selectedOrder.OrderNumber);
                        LoadOrders();
                        MessageBox.Show("Заказ удален", "Успех");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления заказа: {ex.Message}", "Ошибка");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для удаления",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void ButtonRepairAsset_Click(object sender, RoutedEventArgs e)
        {
            if (Assets.SelectedItem is EnterpriseAsset selectedAsset)
            {
                if (selectedAsset.Condition == EnterpriseAsset.Conditions.Broken)
                {
                    MessageBox.Show("Сломанные активы нельзя отремонтировать!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                string action = "";
                string message = "";

                if (selectedAsset.Condition == EnterpriseAsset.Conditions.Damaged)
                {
                    action = "отремонтирован";
                    message = $"Отметить актив '{selectedAsset.Name}' как отремонтированный?";
                }
                else if (selectedAsset.NeedsService())
                {
                    action = "обслужен";
                    message = $"Выполнить плановое обслуживание актива '{selectedAsset.Name}'?\n" +
                             $"Дата обслуживания обновлена.";
                }
                else
                {
                    MessageBox.Show("Этот актив не требует обслуживания или ремонта.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var result = MessageBox.Show(message, "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (selectedAsset.Condition == EnterpriseAsset.Conditions.Damaged)
                        {
                            selectedAsset.Repair();
                            MessageBox.Show($"Актив '{selectedAsset.Name}' {action}.\n" +
                                          $"Новое состояние: {selectedAsset.ConditionDisplay}", "Успех");
                        }
                        else if (selectedAsset.NeedsService())
                        {
                            selectedAsset.LastService = DateTime.Now;
                            MessageBox.Show($"Обслуживание актива '{selectedAsset.Name}' выполнено.\n" +
                                          $"Дата обслуживания: {selectedAsset.LastService:dd.MM.yyyy}", "Успех");
                        }

                        LoadAssets();
                        UpdateStatusBar($"Актив '{selectedAsset.Name}' {action}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите актив для обслуживания или ремонта",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        //панель активы
        private void ButtonAssetDetails_Click(object sender, RoutedEventArgs e)
        {
            if (Assets.SelectedItem is EnterpriseAsset asset)
            {
                MessageBox.Show(asset.ToString(), $"Детали: {asset.Name}");
            }
            else
            {
                MessageBox.Show("Выберите актив для просмотра деталей", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void ButtonDeleteAsset_Click(object sender, RoutedEventArgs e)
        {
            if (Assets.SelectedItem is EnterpriseAsset asset)
            {
                var result = MessageBox.Show($"Удалить '{asset.Name}'?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        storage.DeleteItem(asset.ID);
                        LoadAssets();
                        MessageBox.Show("Актив удален", "Успех");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите актив для удаления", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


    }
    public class QuantityInputWindow : Window
    {
        public decimal SelectedQuantity { get; private set; }
        private TextBox txtQuantity;

        public QuantityInputWindow(string productName, decimal availableQuantity)
        {
            InitializeComponent(productName, availableQuantity);
        }

        private void InitializeComponent(string productName, decimal availableQuantity)
        {
            this.Title = "Введите количество";
            this.Width = 300;
            this.Height = 180;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ResizeMode = ResizeMode.NoResize;

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            // Информация
            var infoText = new TextBlock
            {
                Text = $"Товар: {productName}\nДоступно: {availableQuantity}",
                Margin = new Thickness(10),
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(infoText, 0);

            // Поле ввода
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Количество:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            });

            txtQuantity = new TextBox
            {
                Width = 100,
                Text = "1"
            };
            stackPanel.Children.Add(txtQuantity);
            Grid.SetRow(stackPanel, 1);

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            var btnCancel = new Button
            {
                Content = "Отмена",
                Width = 80,
                Margin = new Thickness(0, 0, 10, 0)
            };
            btnCancel.Click += (s, e) => { this.DialogResult = false; this.Close(); };

            var btnOk = new Button
            {
                Content = "OK",
                Width = 80
            };
            btnOk.Click += BtnOk_Click;

            buttonPanel.Children.Add(btnCancel);
            buttonPanel.Children.Add(btnOk);
            Grid.SetRow(buttonPanel, 2);

            grid.Children.Add(infoText);
            grid.Children.Add(stackPanel);
            grid.Children.Add(buttonPanel);

            this.Content = grid;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtQuantity.Text, out decimal quantity) && quantity > 0)
            {
                SelectedQuantity = quantity;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Введите корректное количество", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}