using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace Warehouse.Front.Controls
{
    public partial class PostConditionDisplay : UserControl
    {
        public PostConditionDisplay()
        {
            InitializeComponent();
        }

        public void SetConditions(List<PostCondition> conditions)
        {
            ConditionsList.ItemsSource = conditions;
        }
    }

    public class PostCondition
    {
        public string Status { get; set; }
        public string Description { get; set; }
        public Brush StatusColor => Status == "✓" ? Brushes.Green : Brushes.Red;
    }
}