using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubscriptionTracker.Models;
using System.Threading.Tasks;

namespace SubscriptionTracker.ViewModels
{
    public partial class SubscriptionItemViewModel : ObservableObject
    {
        private readonly Subscription _subscription;
        private readonly System.Func<Task> _onRefresh;

        [ObservableProperty]
        private bool _isExpanded;

        public SubscriptionItemViewModel(Subscription subscription, System.Func<Task> onRefresh = null)
        {
            _subscription = subscription;
            _onRefresh = onRefresh;
        }

        public Subscription Model => _subscription;

        public string Name => _subscription.Name;
        public string Category => _subscription.Category?.Name ?? "Brak";
        public string Cycle => _subscription.Cycle;
        
        public string FormattedPrice => IsShared ? $"{SplitPrice:0.00} PLN / os. (pełna: {_subscription.Price:0.00} PLN)" : $"{_subscription.Price:0.00} {Services.AppSettings.DefaultCurrency}";
        
        public string Initials => string.IsNullOrWhiteSpace(Name) ? "?" : Name.Substring(0, 1).ToUpper();
        public string IconColor => _subscription.Category?.Color ?? "#808080";
        
        public string StartDateFormatted => _subscription.StartDate.ToString("dd.MM.yyyy");
        public string NextPaymentDateFormatted => _subscription.NextPaymentDate.ToString("dd.MM.yyyy");
        
        public string Status => _subscription.Status ?? "Aktywna";
        public string StatusText => Status == "Aktywna" ? "Aktywna" : "Wstrzymana";
        public string StatusBrush => Status == "Aktywna" ? "#22c55e" : "#ef4444";
        
        // Cost sharing
        public bool IsShared => _subscription.IsShared;
        public int NumberOfMembers => _subscription.NumberOfMembers;
        public string SharedWith => _subscription.SharedWith;
        public decimal SplitPrice => _subscription.SplitPrice;

        public string Note
        {
            get => _subscription.Note ?? "";
            set
            {
                if (_subscription.Note != value)
                {
                    _subscription.Note = value;
                    OnPropertyChanged(nameof(Note));
                }
            }
        }

        [RelayCommand]
        private void ToggleExpand()
        {
            IsExpanded = !IsExpanded;
        }

        [RelayCommand]
        private async Task ToggleStatusAsync()
        {
            _subscription.Status = Status == "Aktywna" ? "Nieaktywna" : "Aktywna";
            
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusBrush));

            var service = new Services.DataService();
            await service.UpdateSubscriptionAsync(_subscription);
        }

        [RelayCommand]
        private async Task SaveNoteAsync()
        {
            var service = new Services.DataService();
            await service.UpdateSubscriptionAsync(_subscription);
            CustomMessageBox.Show("Notatka została pomyślnie zapisana!", "Zapisano", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task LogPaymentAsync()
        {
            var service = new Services.DataService();
            bool success = await service.LogSubscriptionPaymentAsync(_subscription.Id, null);
            if (success)
            {
                OnPropertyChanged(nameof(NextPaymentDateFormatted));
                CustomMessageBox.Show($"Płatność została pomyślnie zarejestrowana! Następna płatność: {NextPaymentDateFormatted}.", "Płatność Zarejestrowana", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                
                if (_onRefresh != null)
                {
                    await _onRefresh();
                }
            }
        }
    }
}
