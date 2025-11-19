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

using Warehouse.Front.Pages;

namespace Warehouse.Front
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Content = new ProductsPage();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new HelpPage();
        }
        private void Products_Click(object sender, RoutedEventArgs e) => MainFrame.Content = new ProductsPage();
        private void Receive_Click(object sender, RoutedEventArgs e) => MainFrame.Content = new ReceiveGoodsPage();
        private void WriteOff_Click(object sender, RoutedEventArgs e) => MainFrame.Content = new WriteOffPage();
        private void Inventory_Click(object sender, RoutedEventArgs e) => MainFrame.Content = new InventoryPage();
        private void Operations_Click(object sender, RoutedEventArgs e) => MainFrame.Content = new OperationsPage();
    }
}
