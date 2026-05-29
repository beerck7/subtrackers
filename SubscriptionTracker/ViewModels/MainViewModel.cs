using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubscriptionTracker.Services;
using System.Windows.Controls;

namespace SubscriptionTracker.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private UserControl _currentView;

        [ObservableProperty]
        private string _activeViewName;

        [ObservableProperty]
        private bool _isLoggedIn = false;

        public MainViewModel()
        {
            // Initial state: Not logged in, show the login view
            ShowLoginScreen();
        }

        private void ShowLoginScreen()
        {
            IsLoggedIn = false;
            ActiveViewName = "Login";
            CurrentView = new LoginView { DataContext = new LoginViewModel(OnLoginSuccess) };
        }

        private void OnLoginSuccess()
        {
            IsLoggedIn = true;
            Navigate("Dashboard");
        }

        [RelayCommand]
        private void Navigate(string viewName)
        {
            if (!IsLoggedIn) return; // Prevent navigation if not authenticated

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
            // Reset the active user session and redirect to the login screen
            SessionManager.CurrentUser = null;
            ShowLoginScreen();
        }
    }
}
