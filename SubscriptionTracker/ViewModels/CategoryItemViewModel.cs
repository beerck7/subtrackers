using CommunityToolkit.Mvvm.ComponentModel;
using SubscriptionTracker.Models;
using System.Linq;

namespace SubscriptionTracker.ViewModels
{
    public partial class CategoryItemViewModel : ObservableObject
    {
        private readonly Category _category;

        public CategoryItemViewModel(Category category)
        {
            _category = category;
        }

        public Category Model => _category;

        public string Name => _category.Name;
        public string Color => _category.Color;

        public int SubscriptionsCount => _category.Subscriptions.Count;
        public string SubscriptionsCountText => $"{SubscriptionsCount} Usług";

        public decimal TotalCost => _category.Subscriptions.Sum(s => s.Price);
        public string FormattedTotalCost => $"{TotalCost:0.00} {Services.AppSettings.DefaultCurrency}";
    }
}
