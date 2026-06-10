using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubscriptionTracker.Services;
using System.Windows.Controls;
using System;
using System.Threading.Tasks;

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

        public event Action StartTransitionAnimation;
        public event Action EndTransitionAnimation;

        public MainViewModel()
        {
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
            NavigateAsync("Dashboard").ConfigureAwait(false);
        }

        [RelayCommand]
        private async Task NavigateAsync(string viewName)
        {
            if (!IsLoggedIn || ActiveViewName == viewName) return;

            StartTransitionAnimation?.Invoke();
            await Task.Delay(250);

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
                case "Family":
                    CurrentView = new FamilyView();
                    break;
            }

            EndTransitionAnimation?.Invoke();
        }

        [RelayCommand]
        private void Shutdown()
        {
            SessionManager.CurrentUser = null;
            ShowLoginScreen();
        }
    }
}
