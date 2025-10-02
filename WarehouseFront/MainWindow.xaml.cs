using System;
using System.Windows;
using System.Linq;
using System.Globalization;
using Back.Сервисы;
using Back.Репозитории;

namespace WarehouseFront
{
    public partial class MainWindow : Window
    {
        private WarehouseService _service;

        public MainWindow()
        {
            InitializeComponent();
            _service = new WarehouseService(new WarehouseRepository());
            InitializeFormDefaults();
            LoadData();
        }

        private void InitializeFormDefaults()
        {
            // Устанавливаем значения по умолчанию для формы приемки
            SkuBox.Text = $"SKU{DateTime.Now:HHmmss}";
            QuantityBox.Text = "10";
            PriceBox.Text = "150,75"; // Теперь с запятой
            NameBox.Text = "Новый товар";
            SupplierBox.Text = "ООО 'Поставщик'";
            DeliveryDate.SelectedDate = DateTime.Now;

            // Устанавливаем значения по умолчанию для формы списания
            WriteOffSkuBox.Text = "SKU001";
            WriteOffQuantityBox.Text = "5";
            ReasonBox.Text = "Бракованная продукция";
            WriteOffDate.SelectedDate = DateTime.Now;
        }

        private void LoadData()
        {
            try
            {
                var products = _service.GetAllProducts().ToList();
                var operations = _service.GetOperationHistory().ToList();

                ProductsGrid.ItemsSource = products;
                OperationsGrid.ItemsSource = operations;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReceiveGoods_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SkuBox.Text) ||
                    string.IsNullOrWhiteSpace(NameBox.Text) ||
                    string.IsNullOrWhiteSpace(QuantityBox.Text) ||
                    string.IsNullOrWhiteSpace(PriceBox.Text) ||
                    string.IsNullOrWhiteSpace(SupplierBox.Text) ||
                    DeliveryDate.SelectedDate == null)
                {
                    MessageBox.Show("❌ Заполните все поля!", "Внимание",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Парсим цену с поддержкой запятых
                decimal цена;
                if (!decimal.TryParse(PriceBox.Text.Replace('.', ','), NumberStyles.Any, CultureInfo.CurrentCulture, out цена))
                {
                    MessageBox.Show("❌ Неверный формат цены! Используйте запятую для дробной части.", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _service.ПринятьТовар(
                    SkuBox.Text.Trim(),
                    NameBox.Text.Trim(),
                    int.Parse(QuantityBox.Text),
                    цена,
                    SupplierBox.Text.Trim(),
                    DeliveryDate.SelectedDate ?? DateTime.Now
                );

                MessageBox.Show("✅ Товар успешно принят на склад!", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
                ClearReceiveForm();
            }
            catch (FormatException)
            {
                MessageBox.Show("❌ Проверьте правильность ввода чисел!", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WriteOffGoods_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(WriteOffSkuBox.Text) ||
                    string.IsNullOrWhiteSpace(WriteOffQuantityBox.Text) ||
                    string.IsNullOrWhiteSpace(ReasonBox.Text) ||
                    WriteOffDate.SelectedDate == null)
                {
                    MessageBox.Show("❌ Заполните все поля!", "Внимание",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _service.СписатьТовар(
                    WriteOffSkuBox.Text.Trim(),
                    int.Parse(WriteOffQuantityBox.Text),
                    ReasonBox.Text.Trim(),
                    WriteOffDate.SelectedDate ?? DateTime.Now
                );

                MessageBox.Show("✅ Товар успешно списан со склада!", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
                ClearWriteOffForm();
            }
            catch (FormatException)
            {
                MessageBox.Show("❌ Проверьте правильность ввода чисел!", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearReceiveForm()
        {
            SkuBox.Text = $"SKU{DateTime.Now:HHmmss}";
            NameBox.Text = "";
            QuantityBox.Text = "1";
            PriceBox.Text = "100,00"; // Теперь с запятой
            SupplierBox.Text = "";
            DeliveryDate.SelectedDate = DateTime.Now;
        }

        private void ClearWriteOffForm()
        {
            WriteOffSkuBox.Text = "";
            WriteOffQuantityBox.Text = "1";
            ReasonBox.Text = "";
            WriteOffDate.SelectedDate = DateTime.Now;
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.Source is System.Windows.Controls.TabControl)
            {
                LoadData();
            }
        }
    }
}