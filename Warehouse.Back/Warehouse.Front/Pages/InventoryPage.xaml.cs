using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using Lab_2.Models;
using Warehouse.Front.Services;

namespace Warehouse.Front.Pages
{
    public partial class InventoryPage : Page
    {
        public InventoryPage()
        {
            InitializeComponent();
            dpAdjustmentDate.DisplayDateEnd = DateTime.Now.Date;
            dpAdjustmentDate.SelectedDate = DateTime.Now.Date;
            LoadSkuList();
        }

        private void LoadSkuList()
        {
            try
            {
                var products = WarehouseConnector.Service.GetAllProducts()?.ToList() ?? Enumerable.Empty<Product>().ToList();
                // Покажем в ComboBox "SKU — Name", но SelectedValuePath = "SKU"
                SkuCombo.ItemsSource = products.Select(p => new { Display = $"{p.SKU} — {p.Name}", p.SKU, p.Quantity }).ToList();
                SkuCombo.DisplayMemberPath = "Display";
                SkuCombo.SelectedValuePath = "SKU";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить товары: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // При выборе SKU показываем текущий остаток
        private void SkuCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtCurrentQty.Text = "—";
            txtActualQty.Text = string.Empty;
            txtReason.Text = string.Empty;
            txtDiff.Text = string.Empty;

            if (SkuCombo.SelectedValue == null) return;

            string sku = SkuCombo.SelectedValue.ToString();
            var product = WarehouseConnector.Service.GetAllProducts().FirstOrDefault(p => p.SKU == sku);
            if (product != null)
            {
                txtCurrentQty.Text = product.Quantity.ToString();
            }
        }

        // Ограничение ввода только цифр
        private static readonly Regex _intRegex = new Regex("[^0-9]+");
        private void Integer_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _intRegex.IsMatch(e.Text);
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            SkuCombo.SelectedIndex = -1;
            txtActualQty.Clear();
            txtReason.Clear();
            txtCurrentQty.Text = "—";
            txtDiff.Text = string.Empty;
            dpAdjustmentDate.SelectedDate = DateTime.Now.Date;
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SkuCombo.SelectedValue == null)
                {
                    MessageBox.Show("Выберите товар.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtActualQty.Text, out int actual) || actual < 0)
                {
                    MessageBox.Show("Введите корректное фактическое количество (>= 0).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string reason = txtReason.Text?.Trim();
                if (string.IsNullOrWhiteSpace(reason))
                {
                    MessageBox.Show("Укажите причину расхождения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime date = dpAdjustmentDate.SelectedDate ?? DateTime.Now;
                if (date > DateTime.Now)
                {
                    MessageBox.Show("Дата инвентаризации не может быть в будущем.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string sku = SkuCombo.SelectedValue.ToString();
                var product = WarehouseConnector.Service.GetAllProducts().FirstOrDefault(p => p.SKU == sku);
                if (product == null)
                {
                    MessageBox.Show("Товар не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int oldQty = product.Quantity;
                int difference = actual - oldQty;

                // Вызов бекенда (метод ничего не возвращает)
                WarehouseConnector.Service.InventoryAdjustment(sku, actual, reason, date);

                // Показываем результат
                txtDiff.Text = $"Корректировка применена. Старый остаток: {oldQty}, Новый: {actual}, Разница: {difference}";

                MessageBox.Show("Инвентаризация успешно сохранена.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

                // Обновляем ProductsPage
                var mw = Application.Current.MainWindow as MainWindow;
                if (mw != null) mw.MainFrame.Content = new ProductsPage();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

