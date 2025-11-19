using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Warehouse.Back.Models;
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

            // Инициализация панели постусловий
            InitializePostConditionsPanel();
        }

        private void InitializePostConditionsPanel()
        {
            // Скрываем панель постусловий до выполнения операции
            PostConditionsPanel.Visibility = Visibility.Collapsed;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Сбрасываем отображение постусловий
            PostConditionsPanel.Visibility = Visibility.Collapsed;
            PostConditionsList.ItemsSource = null;

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

            // Вызов backend сервиса
            try
            {
                var result = WarehouseConnector.Service.ReceiveGoods(sku, name, quantity, price, supplier, deliveryDate);

                if (result.Success)
                {
                    MessageBox.Show(result.Message, "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Отображаем постусловия
                    ShowPostConditions(result.PostConditions);

                    // Очистить поля
                    ClearFields();

                    // Обновить/перейти на страницу товаров
                    var mw = Application.Current.MainWindow as MainWindow;
                    if (mw != null)
                        mw.MainFrame.Content = new ProductsPage();
                }
                else
                {
                    MessageBox.Show(result.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Показываем постусловия даже при ошибке (если есть)
                    if (result.PostConditions != null && result.PostConditions.Any())
                    {
                        ShowPostConditions(result.PostConditions);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowPostConditions(List<PostConditionInfo> postConditions)
        {
            if (postConditions == null || !postConditions.Any())
                return;

            // Создаем список для отображения
            var displayConditions = new List<PostConditionDisplayItem>();

            foreach (var condition in postConditions)
            {
                displayConditions.Add(new PostConditionDisplayItem
                {
                    Description = condition.Description,
                    IsSatisfied = condition.IsSatisfied,
                    Details = condition.Details
                });
            }

            PostConditionsList.ItemsSource = displayConditions;
            PostConditionsPanel.Visibility = Visibility.Visible;
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            PostConditionsPanel.Visibility = Visibility.Collapsed;
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

        // Обработка вставки текста (предотвращение вставки недопустимых символов)
        private void Integer_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (_intRegex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void Decimal_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (_decimalRegex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }

    // Класс для отображения постусловий в UI
    public class PostConditionDisplayItem
    {
        public string Description { get; set; }
        public bool IsSatisfied { get; set; }
        public string Details { get; set; }
        public string StatusSymbol => IsSatisfied ? "✓" : "✗";
        public Brush StatusColor => IsSatisfied ? Brushes.Green : Brushes.Red;
        public string StatusText => IsSatisfied ? "ВЫПОЛНЕНО" : "НЕ ВЫПОЛНЕНО";
    }
}