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
using Warehouse.Front.Services;
using Lab_2.Models; // для Product, если нужно

namespace Warehouse.Front.Pages
{
    public partial class WriteOffPage : Page
    {
        public WriteOffPage()
        {
            InitializeComponent();
            LoadSkuList();
            dpWriteOffDate.DisplayDateEnd = DateTime.Now.Date; // запретить выбор будущей даты
            dpWriteOffDate.SelectedDate = DateTime.Now.Date;
        }

        private void LoadSkuList()
        {
            try
            {
                // Используем правильный метод
                var products = WarehouseConnector.Service.GetAllProducts()?.ToList() ?? Enumerable.Empty<Product>().ToList();

                // Можно отображать Name, но важно, чтобы SelectedValuePath соответствовал свойству модели
                SkuBox.ItemsSource = products;
                SkuBox.DisplayMemberPath = "Name";   // показываем имя товара
                SkuBox.SelectedValuePath = "SKU";    // возвращаем SKU при выборе (обязательно "SKU" с правильным регистром)
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке товаров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Разрешаем ввод только чисел
        private void QuantityBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            SkuBox.SelectedIndex = -1;
            QuantityBox.Clear();
            ReasonBox.Clear();
            dpWriteOffDate.SelectedDate = DateTime.Now.Date;
        }

        private void BtnWriteOff_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SkuBox.SelectedValue == null)
                {
                    MessageBox.Show("Выберите товар для списания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(QuantityBox.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Введите корректное количество (>0).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string reason = ReasonBox.Text?.Trim();
                if (string.IsNullOrWhiteSpace(reason))
                {
                    MessageBox.Show("Укажите причину списания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime date = dpWriteOffDate.SelectedDate ?? DateTime.Now;
                if (date > DateTime.Now)
                {
                    MessageBox.Show("Дата списания не может быть в будущем.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string sku = SkuBox.SelectedValue.ToString(); // здесь будет значение свойства SKU

                // Вызов backend — метод возвращает void, поэтому просто вызываем
                WarehouseConnector.Service.WriteOffGoods(sku, quantity, reason, date);

                MessageBox.Show("Списание выполнено успешно.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

                // Обновляем список товаров/возвращаем на страницу товаров
                var mw = Application.Current.MainWindow as MainWindow;
                if (mw != null)
                    mw.MainFrame.Content = new ProductsPage();
                else
                    // попытка вернуть в навигацию, если доступна
                    NavigationService?.Navigate(new ProductsPage());
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
