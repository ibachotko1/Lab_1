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
using Lab_2.Models;
using Warehouse.Front.Services;

namespace Warehouse.Front.Pages
{
    public partial class OperationsPage : Page
    {
        private IQueryable<OperationRecord> _allOps;

        public OperationsPage()
        {
            InitializeComponent();
            LoadOperations();
        }

        private void LoadOperations()
        {
            try
            {
                var ops = WarehouseConnector.Service.GetAllOperations()?.ToList() ?? Enumerable.Empty<OperationRecord>().ToList();
                // Сохраняем для фильтрации
                _allOps = ops.AsQueryable();
                dgOperations.ItemsSource = _allOps.OrderByDescending(o => o.RecordDate).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке операций: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOperations();
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            string sku = txtFilterSku.Text?.Trim();
            if (string.IsNullOrEmpty(sku))
            {
                dgOperations.ItemsSource = _allOps.OrderByDescending(o => o.RecordDate).ToList();
                return;
            }

            var filtered = _allOps.Where(o => o.SKU != null && o.SKU.IndexOf(sku, StringComparison.OrdinalIgnoreCase) >= 0)
                                  .OrderByDescending(o => o.RecordDate)
                                  .ToList();

            dgOperations.ItemsSource = filtered;
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            txtFilterSku.Clear();
            dgOperations.ItemsSource = _allOps.OrderByDescending(o => o.RecordDate).ToList();
        }
    }
}
