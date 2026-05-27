using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubscriptionTracker.Models;
using SubscriptionTracker.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SubscriptionTracker.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly DataService _dataService;

        [ObservableProperty]
        private string _monthlyCostsText = "0,00 PLN";

        [ObservableProperty]
        private string _activeSubscriptionsCountText = "0 Usług";

        [ObservableProperty]
        private string _newSubscriptionsThisMonthText = "0 nowych w tym miesiącu";

        [ObservableProperty]
        private string _todayDateText = "Dzisiejsza data: ";

        [ObservableProperty]
        private ObservableCollection<SubscriptionItemViewModel> _upcomingPayments;

        [ObservableProperty]
        private ObservableCollection<CategoryExpenseItem> _categoryExpenses;

        public DashboardViewModel()
        {
            _dataService = new DataService();
            UpcomingPayments = new ObservableCollection<SubscriptionItemViewModel>();
            CategoryExpenses = new ObservableCollection<CategoryExpenseItem>();

            LoadDashboardDataCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadDashboardDataAsync()
        {
            var subs = await _dataService.GetSubscriptionsAsync();
            var activeSubs = subs.Where(s => s.Status == "Aktywna" || string.IsNullOrEmpty(s.Status)).ToList();

            decimal totalMonthly = 0;
            foreach (var sub in activeSubs)
            {
                if (sub.Cycle == "Rocznie")
                {
                    totalMonthly += sub.Price / 12;
                }
                else
                {
                    totalMonthly += sub.Price;
                }
            }
            MonthlyCostsText = $"{totalMonthly:0.00} PLN";

            ActiveSubscriptionsCountText = $"{activeSubs.Count} Usług";

            var now = DateTime.Now;
            TodayDateText = $"Dzisiejsza data: {now.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("pl-PL"))}";
            int newThisMonth = activeSubs.Count(s => s.StartDate.Month == now.Month && s.StartDate.Year == now.Year);
            NewSubscriptionsThisMonthText = $"{newThisMonth} nowe w tym miesiącu";

            var upcoming = activeSubs
                .OrderBy(s => s.NextPaymentDate)
                .Take(3)
                .Select(s => new SubscriptionItemViewModel(s))
                .ToList();

            UpcomingPayments.Clear();
            foreach (var item in upcoming)
            {
                UpcomingPayments.Add(item);
            }

            CategoryExpenses.Clear();
            if (totalMonthly > 0)
            {
                var categories = await _dataService.GetCategoriesAsync();
                foreach (var cat in categories)
                {
                    decimal catMonthly = 0;
                    foreach (var sub in cat.Subscriptions.Where(s => s.Status == "Aktywna" || string.IsNullOrEmpty(s.Status)))
                    {
                        if (sub.Cycle == "Rocznie")
                        {
                            catMonthly += sub.Price / 12;
                        }
                        else
                        {
                            catMonthly += sub.Price;
                        }
                    }

                    if (catMonthly > 0)
                    {
                        decimal pct = (catMonthly / totalMonthly) * 100;
                        CategoryExpenses.Add(new CategoryExpenseItem
                        {
                            Name = cat.Name,
                            Color = cat.Color ?? "#808080",
                            Percentage = (double)pct,
                            FormattedCostWithPercentage = $"{catMonthly:0.00} PLN ({pct:0}%)"
                        });
                    }
                }
            }
        }
    }

    public class CategoryExpenseItem
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public double Percentage { get; set; }
        public string FormattedCostWithPercentage { get; set; }
    }
}
