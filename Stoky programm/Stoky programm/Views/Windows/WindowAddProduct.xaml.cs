using Stoky_programm.Models;
using Stoky_programm.Windows;
using System;
using System.Linq;
using System.Windows;

namespace Stoky_programm.Windows
{
    public partial class WindowAddProduct : Window
    {
        public Products NewProduct { get; private set; }

        public WindowAddProduct()
        {
            InitializeComponent();
            InitializeCategoryComboBox();
        }

        private void InitializeCategoryComboBox()
        {
            cmbCategory.ItemsSource = Enum.GetValues(typeof(Products.ProductCategory))
                .Cast<Products.ProductCategory>()
                .ToList();
            cmbCategory.SelectedIndex = 0;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите название продукта", "Ошибка");
                    return;
                }

                if (!decimal.TryParse(txtQuantity.Text, out decimal quantity) || quantity <= 0)
                {
                    MessageBox.Show("Введите корректное количество", "Ошибка");
                    return;
                }

                if (!double.TryParse(txtPrice.Text, out double price) || price <= 0)
                {
                    MessageBox.Show("Введите корректную цену", "Ошибка");
                    return;
                }
                DateTime productionDate = ProductionDate.SelectedDate ?? DateTime.Now;
                DateTime addedDate = AddedDate.SelectedDate ?? DateTime.Now;
                if (productionDate > addedDate)
                {
                    MessageBox.Show("Дата производства не может быть позже даты добавления на склад!",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Создаем новый продукт
                NewProduct = new Products(
                txtName.Text,
                MaterialObj.UnitType.Piece,
                quantity,
                price,
                addedDate,                    
                (Products.ProductCategory)cmbCategory.SelectedItem,
                txtManufacturer.Text,
                txtModel.Text,
                productionDate         
                );

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}