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
using System.Globalization;
using System.Text.RegularExpressions;
using Warehouse.Front.Services;

namespace Warehouse.Front.Pages
{
    public partial class ReceiveGoodsPage : Page
    {
        public ReceiveGoodsPage()
        {
            InitializeComponent();

            // Инициализация даты: нельзя выбрать дату в будущем
            dpDeliveryDate.SelectedDate = DateTime.Now.Date;
            dpDeliveryDate.DisplayDateEnd = DateTime.Now.Date;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Простая client-side валидация
            var sku = txtSku.Text?.Trim();
            var name = txtName.Text?.Trim();
            var supplier = txtSupplier.Text?.Trim();

            if (string.IsNullOrEmpty(sku))
            {
                MessageBox.Show("SKU не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Название не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text?.Trim(), out int quantity))
            {
                MessageBox.Show("Некорректное количество. Введите целое число.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (quantity <= 0)
            {
                MessageBox.Show("Количество должно быть больше 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text?.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out decimal price))
            {
                MessageBox.Show("Некорректная цена. Введите число (напр. 123.45).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (price <= 0)
            {
                MessageBox.Show("Цена должна быть больше 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(supplier))
            {
                MessageBox.Show("Поставщик не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var deliveryDate = dpDeliveryDate.SelectedDate ?? DateTime.Now.Date;
            if (deliveryDate > DateTime.Now)
            {
                MessageBox.Show("Дата поставки не может быть в будущем.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Вызов backend сервиса с обработкой исключений
            try
            {
                WarehouseConnector.Service.ReceiveGoods(sku, name, quantity, price, supplier, deliveryDate);

                MessageBox.Show("Товар успешно добавлен.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

                // Очистить поля
                ClearFields();

                // Обновить/перейти на страницу товаров (создаём новый экземпляр — в его конструкторе данные автоматически загрузятся)
                var mw = Application.Current.MainWindow as MainWindow;
                if (mw != null)
                    mw.MainFrame.Content = new ProductsPage();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show(ex.Message, "Неверные данные", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Неожиданная ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            txtSku.Text = string.Empty;
            txtName.Text = string.Empty;
            txtQuantity.Text = string.Empty;
            txtPrice.Text = string.Empty;
            txtSupplier.Text = string.Empty;
            dpDeliveryDate.SelectedDate = DateTime.Now.Date;
        }

        // Разрешаем ввод только цифр для целых
        private static readonly Regex _intRegex = new Regex("[^0-9]+");
        private void Integer_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _intRegex.IsMatch(e.Text);
        }

        // Для цены разрешаем цифры и разделитель (запятая/точка)
        private static readonly Regex _decimalRegex = new Regex("[^0-9\\,\\.]");
        private void Decimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _decimalRegex.IsMatch(e.Text);
        }
    }
}

