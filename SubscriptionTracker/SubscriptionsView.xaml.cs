using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SubscriptionTracker
{
    public partial class SubscriptionsView : UserControl
    {
        private ObservableCollection<SubscriptionItem> _allSubscriptions;
        public ObservableCollection<SubscriptionItem> DisplayedSubscriptions { get; set; }

        public SubscriptionsView()
        {
            InitializeComponent();
            _allSubscriptions = new ObservableCollection<SubscriptionItem>
            {
                new SubscriptionItem { Initials="N", IconColor="#e50914", Name="Netflix Premium", Category="Streaming", Cycle="Miesięcznie", Price=60.00m },
                new SubscriptionItem { Initials="A", IconColor="#ff0000", Name="Adobe Creative Cloud", Category="Oprogramowanie", Cycle="Miesięcznie", Price=249.99m },
                new SubscriptionItem { Initials="S", IconColor="#22c55e", Name="Spotify Premium", Category="Streaming", Cycle="Miesięcznie", Price=19.99m },
                new SubscriptionItem { Initials="G", IconColor="#333333", Name="GitHub Pro", Category="Programowanie", Cycle="Rocznie", Price=30.00m }
            };

            DisplayedSubscriptions = new ObservableCollection<SubscriptionItem>(_allSubscriptions);
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
                _allSubscriptions.Add(addWindow.NewSubscription);
                DisplayedSubscriptions.Add(addWindow.NewSubscription);
            }
        }
    }
}
