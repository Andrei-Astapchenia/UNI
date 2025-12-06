using Stoky_programm.Data;
using Stoky_programm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Stoky_programm
{
    public partial class MainWindow : Window
    {
        private Storage storage;

        public MainWindow()
        {
            InitializeComponent();
            storage = new Storage();
            LoadAllItems();
        }

        private void LoadAllItems()
        {
            Items.ItemsSource = storage.GetAllItems();
            TblStatus.Text = $"Всего объектов: {storage.GetAllItems().Count}";
        }

        private void LoadProducts()
        {
            Items.ItemsSource = storage.GetAllProducts();
            TblStatus.Text = $"Продуктов: {storage.GetAllProducts().Count}";
        }

        private void LoadAssets()
        {
            Items.ItemsSource = storage.GetAllAssets();
            TblStatus.Text = $"Активов: {storage.GetAllAssets().Count}";
        }

        private void BottonAddProduct_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Здесь будет окно добавления продукта", "Добавление");
            
        }

        private void BottonAddAsset_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Здесь будет окно добавления актива", "Добавление");

        }

        private void BottonShowAll_Click(object sender, RoutedEventArgs e)
        {
            LoadAllItems();
        }

        private void BottonShowProducts_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void BottonShowAssets_Click(object sender, RoutedEventArgs e)
        {
            LoadAssets();
        }

        private void BottonDetails_Click(object sender, RoutedEventArgs e)
        {
            if (Items.SelectedItem is MaterialObj item)
            {
                MessageBox.Show(item.ToString(), $"Детали: {item.Name}");
            }
        }

        private void BottonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (Items.SelectedItem is MaterialObj item)
            {
                var result = MessageBox.Show($"Удалить '{item.Name}'?",
                    "Подтверждение", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        storage.DeleteItem(item.ID);
                        LoadAllItems();
                        MessageBox.Show("Объект удален");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            }
        }
    }
}