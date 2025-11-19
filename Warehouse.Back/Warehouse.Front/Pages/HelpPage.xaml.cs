using System.Windows.Controls;
using Warehouse.Front.Services;

namespace Warehouse.Front.Pages
{
    public partial class HelpPage : Page
    {
        public HelpPage()
        {
            InitializeComponent();
            InitializeHelpTopics();
        }

        private void InitializeHelpTopics()
        {
            var topics = new[]
            {
                new { Display = "Главная страница", Value = "main" },
                new { Display = "Товары", Value = "products" },
                new { Display = "Приемка товара", Value = "receive" },
                new { Display = "Списание товара", Value = "writeoff" },
                new { Display = "Инвентаризация", Value = "inventory" },
                new { Display = "Журнал операций", Value = "operations" }
            };

            HelpTopicCombo.ItemsSource = topics;
            HelpTopicCombo.DisplayMemberPath = "Display";
            HelpTopicCombo.SelectedValuePath = "Value";
            HelpTopicCombo.SelectedIndex = 0;
        }

        private void HelpTopicCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HelpTopicCombo.SelectedValue != null)
            {
                string topic = HelpTopicCombo.SelectedValue.ToString();
                HelpContentText.Text = HelpService.GetHelp(topic);
            }
        }
    }
}
