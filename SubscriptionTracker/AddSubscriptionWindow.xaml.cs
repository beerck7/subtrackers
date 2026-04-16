using System.Windows;
using System.Windows.Controls;

namespace SubscriptionTracker
{
    public partial class AddSubscriptionWindow : Window
    {
        public SubscriptionItem NewSubscription { get; private set; }

        public AddSubscriptionWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            if (string.IsNullOrWhiteSpace(name)) return;

            string category = ((ComboBoxItem)CategoryComboBox.SelectedItem).Content.ToString();
            string cycle = ((ComboBoxItem)CycleComboBox.SelectedItem).Content.ToString();
            decimal.TryParse(PriceTextBox.Text.Replace(".", ","), out decimal price);

            string color = "#3b82f6";
            if (category == "Streaming") color = "#22c55e";
            if (category == "Gry") color = "#eab308";
            if (category == "Oprogramowanie") color = "#ff0000";

            NewSubscription = new SubscriptionItem
            {
                Name = name,
                Category = category,
                Cycle = cycle,
                Price = price,
                Initials = name.Length > 0 ? name.Substring(0, 1).ToUpper() : "?",
                IconColor = color
            };

            DialogResult = true;
        }
    }
}
