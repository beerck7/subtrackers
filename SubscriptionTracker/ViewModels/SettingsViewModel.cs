using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubscriptionTracker.Services;
using SubscriptionTracker.Data;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace SubscriptionTracker.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private bool _isInitialized;

        [ObservableProperty]
        private string _userName;

        [ObservableProperty]
        private string _userEmail;

        [ObservableProperty]
        private bool _isDarkTheme;

        [ObservableProperty]
        private bool _areNotificationsEnabled;

        [ObservableProperty]
        private string _defaultCurrency;

        [ObservableProperty]
        private bool _isEditing;

        public ObservableCollection<string> CurrencyOptions { get; } = new() { "PLN", "USD", "EUR" };

        public SettingsViewModel()
        {
            // Assign fields directly to backing variables to prevent event firing during instantiation
            _userName = AppSettings.UserName;
            _userEmail = AppSettings.UserEmail;
            _isDarkTheme = AppSettings.IsDarkTheme;
            _areNotificationsEnabled = AppSettings.AreNotificationsEnabled;
            _defaultCurrency = AppSettings.DefaultCurrency;
            _isEditing = false;
            
            _isInitialized = true;
        }

        [RelayCommand]
        private void ToggleEdit()
        {
            IsEditing = !IsEditing;
            if (!IsEditing)
            {
                // Revert changes if cancelled
                UserName = AppSettings.UserName;
                UserEmail = AppSettings.UserEmail;
            }
        }

        [RelayCommand]
        private void SaveProfile()
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(UserEmail))
            {
                MessageBox.Show("Nazwa i e-mail nie mogą być puste.", "Błąd zapisu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AppSettings.UserName = UserName;
            AppSettings.UserEmail = UserEmail;
            AppSettings.Save();
            IsEditing = false;

            MessageBox.Show("Profil użytkownika został pomyślnie zaktualizowany!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        partial void OnIsDarkThemeChanged(bool value)
        {
            if (!_isInitialized) return;

            AppSettings.IsDarkTheme = value;
            AppSettings.Save();

            var app = Application.Current;
            if (value)
            {
                ApplyDarkTheme(app);
            }
            else
            {
                ApplyLightTheme(app);
            }
        }

        partial void OnAreNotificationsEnabledChanged(bool value)
        {
            if (!_isInitialized) return;

            AppSettings.AreNotificationsEnabled = value;
            AppSettings.Save();

            string status = value ? "włączone" : "wyłączone";
            MessageBox.Show($"Powiadomienia o płatnościach zostały {status}.", "Ustawienia powiadomień", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        partial void OnDefaultCurrencyChanged(string value)
        {
            if (!_isInitialized) return;

            if (!string.IsNullOrEmpty(value))
            {
                AppSettings.DefaultCurrency = value;
                AppSettings.Save();
            }
        }

        [RelayCommand]
        private async Task DeleteAllDataAsync()
        {
            var result = MessageBox.Show(
                "Czy na pewno chcesz usunąć wszystkie subskrypcje i kategorie z bazy danych?\nTej operacji nie można cofnąć!",
                "Potwierdzenie usunięcia danych",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using (var db = new AppDbContext())
                {
                    db.Subscriptions.RemoveRange(await db.Subscriptions.ToListAsync());
                    db.Categories.RemoveRange(await db.Categories.ToListAsync());
                    
                    // Re-seed default categories
                    db.Categories.Add(new Models.Category { Id = 1, Name = "Streaming", Color = "#22c55e" });
                    db.Categories.Add(new Models.Category { Id = 2, Name = "Muzyka", Color = "#f43f5e" });
                    db.Categories.Add(new Models.Category { Id = 3, Name = "Oprogramowanie", Color = "#3b82f6" });
                    db.Categories.Add(new Models.Category { Id = 4, Name = "Gry", Color = "#eab308" });
                    db.Categories.Add(new Models.Category { Id = 5, Name = "Inne", Color = "#a855f7" });
                    
                    await db.SaveChangesAsync();
                }

                MessageBox.Show("Wszystkie dane zostały wyczyszczone. Przywrócono domyślne kategorie.", "Wyczyszczono dane", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ApplyLightTheme(Application app)
        {
            try
            {
                app.Resources["AppBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F4F6F9"));
                app.Resources["CardBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                app.Resources["CardBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
                app.Resources["InputBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EDF2F7"));
                app.Resources["TextPrimary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A202C"));
                app.Resources["TextSecondary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#718096"));
            }
            catch { }
        }

        private void ApplyDarkTheme(Application app)
        {
            try
            {
                app.Resources["AppBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F0F12"));
                app.Resources["CardBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E24"));
                app.Resources["CardBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A35"));
                app.Resources["InputBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16161B"));
                app.Resources["TextPrimary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                app.Resources["TextSecondary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8A8A93"));
            }
            catch { }
        }
    }
}
