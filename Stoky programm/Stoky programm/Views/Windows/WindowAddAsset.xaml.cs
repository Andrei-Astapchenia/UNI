using Stoky_programm.Data;
using Stoky_programm.Models;
using Stoky_programm.Windows;
using System;
using System.Linq;
using System.Windows;

namespace Stoky_programm.Windows
{
    public partial class WindowAddAsset : Window
    {
        public EnterpriseAsset NewAsset { get; private set; }

        public WindowAddAsset()
        {
            InitializeComponent();
            InitializeComboBoxes();
        }

        private void InitializeComboBoxes()
        {
            cmbAssetType.ItemsSource = Enum.GetValues(typeof(EnterpriseAsset.AssetType))
                .Cast<EnterpriseAsset.AssetType>()
                .ToList();

            cmbCondition.ItemsSource = Enum.GetValues(typeof(EnterpriseAsset.Conditions))
                .Cast<EnterpriseAsset.Conditions>()
                .ToList();

            cmbAssetType.SelectedIndex = 0;
            cmbCondition.SelectedIndex = 1; 
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите название актива", "Ошибка");
                    return;
                }

                if (!int.TryParse(txtInventoryNumber.Text, out int inventoryNumber) || inventoryNumber <= 0)
                {
                    MessageBox.Show("Введите корректный инвентарный номер (положительное целое число)", "Ошибка");
                    return;
                }

                decimal quantity = 1;

                if (!double.TryParse(txtPrice.Text, out double price) || price <= 0)
                {
                    MessageBox.Show("Введите корректную цену", "Ошибка");
                    return;
                }

                NewAsset = new EnterpriseAsset(
                    txtName.Text,
                    MaterialObj.UnitType.Piece,
                    quantity,
                    price,
                    (EnterpriseAsset.AssetType)cmbAssetType.SelectedItem,
                    (EnterpriseAsset.Conditions)cmbCondition.SelectedItem,
                     inventoryNumber,
                    dpCommissioningDate.SelectedDate ?? DateTime.Now
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