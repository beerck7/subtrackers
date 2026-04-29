using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SubscriptionTracker.Models;

namespace SubscriptionTracker
{
    public partial class SubscriptionsView : UserControl
    {
        private ObservableCollection<Subscription> _allSubscriptions;
        public ObservableCollection<Subscription> DisplayedSubscriptions { get; set; }

        public SubscriptionsView()
        {
            InitializeComponent();
            _allSubscriptions = new ObservableCollection<Subscription>();
            DisplayedSubscriptions = new ObservableCollection<Subscription>(_allSubscriptions);
            SubscriptionsList.ItemsSource = DisplayedSubscriptions;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchTextBox.Text?.ToLower() ?? "";
            SearchPlaceholder.Visibility = string.IsNullOrEmpty(query) ? Visibility.Visible : Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(query))
            {
                DisplayedSubscriptions.Clear();
                foreach (var item in _allSubscriptions) DisplayedSubscriptions.Add(item);
            }
            else
            {
                var filtered = _allSubscriptions.Where(s => s.Name.ToLower().Contains(query)).ToList();
                DisplayedSubscriptions.Clear();
                foreach (var item in filtered) DisplayedSubscriptions.Add(item);
            }
        }

        private void AddNewButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddSubscriptionWindow();
            addWindow.Owner = Window.GetWindow(this);
            if (addWindow.ShowDialog() == true)
            {
                // To be implemented in Etap 3
            }
        }
    }
}
