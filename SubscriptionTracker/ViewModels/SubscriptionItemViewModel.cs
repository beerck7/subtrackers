using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubscriptionTracker.Models;
using System.Linq;
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
        public string IconColor => !string.IsNullOrWhiteSpace(_subscription.Color) ? _subscription.Color : (_subscription.Category?.Color ?? "#808080");
        
        public string StartDateFormatted => _subscription.StartDate.ToString("dd.MM.yyyy");
        public string NextPaymentDateFormatted => _subscription.NextPaymentDate.ToString("dd.MM.yyyy");
        
        public string Status => _subscription.Status ?? "Aktywna";
        public string StatusText => Status == "Aktywna" ? "Aktywna" : "Wstrzymana";
        public string StatusBrush => Status == "Aktywna" ? "#22c55e" : "#ef4444";
        

        public bool IsShared => _subscription.IsShared;
        public int NumberOfMembers => _subscription.NumberOfMembers;
        
        public string SharedWith
        {
            get
            {
                var currentUser = Services.SessionManager.CurrentUser;
                if (currentUser == null) return _subscription.SharedWith;

                if (_subscription.UserId == currentUser.Id)
                {
                    return _subscription.SharedWith;
                }
                else
                {
                    var ownerName = _subscription.User?.Username ?? "Właściciel";
                    var coPayers = (_subscription.SharedWith ?? "")
                        .Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                        .Select(n => n.Trim())
                        .Where(n => !string.Equals(n, currentUser.Username, System.StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (coPayers.Any())
                    {
                        return $"{ownerName}, {string.Join(", ", coPayers)}";
                    }
                    return ownerName;
                }
            }
        }

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
