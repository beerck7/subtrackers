using CommunityToolkit.Mvvm.ComponentModel;
using SubscriptionTracker.Models;

namespace SubscriptionTracker.ViewModels
{
    public partial class SubscriptionItemViewModel : ObservableObject
    {
        private readonly Subscription _subscription;

        public SubscriptionItemViewModel(Subscription subscription)
        {
            _subscription = subscription;
        }

        public Subscription Model => _subscription;

        public string Name => _subscription.Name;
        public string Category => _subscription.Category?.Name ?? "Brak";
        public string Cycle => _subscription.Cycle;
        
        public string FormattedPrice => $"{_subscription.Price:0.00} PLN";
        
        public string Initials => string.IsNullOrWhiteSpace(Name) ? "?" : Name.Substring(0, 1).ToUpper();
        public string IconColor => _subscription.Category?.Color ?? "#808080";
        public string NextPaymentDateFormatted => _subscription.NextPaymentDate.ToString("dd.MM.yyyy");
    }
}
