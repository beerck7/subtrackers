using System.Windows;

namespace SubscriptionTracker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new DashboardView();
        }

        private void ShowDashboard(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DashboardView();
        }

        private void ShowSubscriptions(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new SubscriptionsView();
        }

        private void ShowCategories(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new CategoriesView();
        }

        private void ShowSettings(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new SettingsView();
        }
    }
}