using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubscriptionTracker.Models;
using System.Threading.Tasks;

namespace SubscriptionTracker.ViewModels
{
    public partial class SubscriptionItemViewModel : ObservableObject
    {
        private readonly Subscription _subscription;

        [ObservableProperty]
        private bool _isExpanded;

        public SubscriptionItemViewModel(Subscription subscription)
        {
            _subscription = subscription;
        }

        public Subscription Model => _subscription;

        public string Name => _subscription.Name;
        public string Category => _subscription.Category?.Name ?? "Brak";
        public string Cycle => _subscription.Cycle;
        
        public string FormattedPrice => $"{_subscription.Price:0.00} {Services.AppSettings.DefaultCurrency}";
        
        public string Initials => string.IsNullOrWhiteSpace(Name) ? "?" : Name.Substring(0, 1).ToUpper();
        public string IconColor => _subscription.Category?.Color ?? "#808080";
        
        public string StartDateFormatted => _subscription.StartDate.ToString("dd.MM.yyyy");
        public string NextPaymentDateFormatted => _subscription.NextPaymentDate.ToString("dd.MM.yyyy");
        
        public string Status => _subscription.Status ?? "Aktywna";
        public string StatusText => Status == "Aktywna" ? "Aktywna" : "Wstrzymana";
        public string StatusBrush => Status == "Aktywna" ? "#22c55e" : "#ef4444";
        
        public string Note => string.IsNullOrWhiteSpace(_subscription.Note) ? "Brak dodatkowych notatek." : _subscription.Note;

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
    }
}
