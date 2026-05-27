using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace SubscriptionTracker.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private UserControl _currentView;

        [ObservableProperty]
        private string _activeViewName;

        public MainViewModel()
        {
            Navigate("Dashboard");
        }

        [RelayCommand]
        private void Navigate(string viewName)
        {
            ActiveViewName = viewName;
            
            switch (viewName)
            {
                case "Dashboard":
                    CurrentView = new DashboardView();
                    break;
                case "Subscriptions":
                    CurrentView = new SubscriptionsView();
                    break;
                case "Categories":
                    CurrentView = new CategoriesView();
                    break;
                case "Settings":
                    CurrentView = new SettingsView();
                    break;
            }
        }
        [RelayCommand]
        private void Shutdown()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
