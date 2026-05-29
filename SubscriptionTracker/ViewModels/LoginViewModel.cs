using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubscriptionTracker.Services;
using System;
using System.Threading.Tasks;

namespace SubscriptionTracker.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly DataService _dataService;
        private readonly Action _onLoginSuccess;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private bool _isRegisterMode = false;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isStatusError = true;

        public string TitleText => IsRegisterMode ? "Utwórz konto" : "Zaloguj się";
        public string SubmitButtonText => IsRegisterMode ? "Zarejestruj się" : "Zaloguj się";
        public string ToggleText => IsRegisterMode ? "Masz już konto? Zaloguj się" : "Nie masz konta? Zarejestruj się";

        public LoginViewModel(Action onLoginSuccess)
        {
            _dataService = new DataService();
            _onLoginSuccess = onLoginSuccess;
        }

        partial void OnIsRegisterModeChanged(bool value)
        {
            ErrorMessage = string.Empty;
            IsStatusError = true;
            Username = string.Empty;
            Password = string.Empty;
            Email = string.Empty;
            
            OnPropertyChanged(nameof(TitleText));
            OnPropertyChanged(nameof(SubmitButtonText));
            OnPropertyChanged(nameof(ToggleText));
        }

        [RelayCommand]
        private void ToggleMode()
        {
            IsRegisterMode = !IsRegisterMode;
        }

        [RelayCommand]
        private async Task SubmitAsync()
        {
            ErrorMessage = string.Empty;
            IsStatusError = true;

            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Nazwa użytkownika jest wymagana.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Hasło jest wymagane.";
                return;
            }

            if (IsRegisterMode && string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Adres e-mail jest wymagany.";
                return;
            }

            try
            {
                if (IsRegisterMode)
                {
                    // Registration Mode
                    bool registered = await _dataService.RegisterUserAsync(Username, Password, Email);
                    if (registered)
                    {
                        IsRegisterMode = false;
                        IsStatusError = false;
                        ErrorMessage = "Rejestracja udana! Zaloguj się.";
                        Password = string.Empty; // Clear password on success to signal UI
                    }
                    else
                    {
                        ErrorMessage = "Ta nazwa użytkownika jest już zajęta.";
                    }
                }
                else
                {
                    // Login Mode
                    var user = await _dataService.LoginUserAsync(Username, Password);
                    if (user != null)
                    {
                        SessionManager.CurrentUser = user;
                        _onLoginSuccess?.Invoke();
                    }
                    else
                    {
                        ErrorMessage = "Nieprawidłowa nazwa użytkownika lub hasło.";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Wystąpił błąd systemowy: {ex.Message}";
            }
        }
    }
}
