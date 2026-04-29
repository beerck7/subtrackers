using System.Windows;
using System.Windows.Controls;
using SubscriptionTracker.Models;

namespace SubscriptionTracker
{
    public partial class AddSubscriptionWindow : Window
    {
        public Subscription NewSubscription { get; private set; }

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

            NewSubscription = new Subscription
            {
                Name = name,
                Cycle = cycle,
                Price = price
                // The rest will be done in Etap 3
            };

            DialogResult = true;
        }
    }
}
