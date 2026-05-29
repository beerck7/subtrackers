using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubscriptionTracker.Models;
using SubscriptionTracker.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SubscriptionTracker.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly DataService _dataService;
        private decimal _totalMonthlyCostCalculated = 0;
        private List<CategoryExpenseItemRaw> _rawCategoryExpenses = new();

        [ObservableProperty]
        private string _monthlyCostsText = "0,00";

        [ObservableProperty]
        private string _activeSubscriptionsCountText = "0 Usług";

        [ObservableProperty]
        private string _newSubscriptionsThisMonthText = "0 nowych w tym miesiącu";

        [ObservableProperty]
        private string _todayDateText = "Dzisiejsza data: ";

        [ObservableProperty]
        private string _expensePeriodLabel = "Miesięcznie";

        [ObservableProperty]
        private bool _isYearlyView = false;

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
        private void SetMonthly()
        {
            IsYearlyView = false;
        }

        [RelayCommand]
        private void SetYearly()
        {
            IsYearlyView = true;
        }

        [RelayCommand]
        private async Task LoadDashboardDataAsync()
        {
            var subs = await _dataService.GetSubscriptionsAsync();
            var activeSubs = subs.Where(s => s.Status == "Aktywna" || string.IsNullOrEmpty(s.Status)).ToList();

            _totalMonthlyCostCalculated = 0;
            foreach (var sub in activeSubs)
            {
                if (sub.Cycle == "Rocznie")
                {
                    _totalMonthlyCostCalculated += sub.Price / 12;
                }
                else
                {
                    _totalMonthlyCostCalculated += sub.Price;
                }
            }

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

            _rawCategoryExpenses.Clear();
            if (_totalMonthlyCostCalculated > 0)
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
                        _rawCategoryExpenses.Add(new CategoryExpenseItemRaw
                        {
                            Name = cat.Name,
                            Color = cat.Color ?? "#808080",
                            MonthlyCost = catMonthly
                        });
                    }
                }
            }

            UpdateExpenseDisplay();
        }

        partial void OnIsYearlyViewChanged(bool value)
        {
            UpdateExpenseDisplay();
        }

        private void UpdateExpenseDisplay()
        {
            ExpensePeriodLabel = IsYearlyView ? "Rocznie" : "Miesięcznie";
            
            decimal displayTotal = IsYearlyView ? _totalMonthlyCostCalculated * 12 : _totalMonthlyCostCalculated;
            MonthlyCostsText = $"{displayTotal:0.00} {Services.AppSettings.DefaultCurrency}";
            
            CategoryExpenses.Clear();
            if (_totalMonthlyCostCalculated > 0)
            {
                double currentAngle = -Math.PI / 2.0; // Start at top
                foreach (var raw in _rawCategoryExpenses)
                {
                    decimal displayCost = IsYearlyView ? raw.MonthlyCost * 12 : raw.MonthlyCost;
                    double pct = (double)((raw.MonthlyCost / _totalMonthlyCostCalculated) * 100M);
                    
                    var item = new CategoryExpenseItem
                    {
                        Name = raw.Name,
                        Color = raw.Color,
                        Percentage = pct,
                        FormattedCostWithPercentage = $"{displayCost:0.00} {Services.AppSettings.DefaultCurrency} ({pct:0}%)"
                    };
                    
                    // Arc path calculation
                    double sweepAngle = (pct / 100.0) * 2.0 * Math.PI;
                    double cx = 75.0;
                    double cy = 75.0;
                    double r = 60.0;
                    
                    double startAngle = currentAngle;
                    double endAngle = currentAngle + sweepAngle;
                    
                    if (pct >= 99.9)
                    {
                        endAngle = startAngle + 2.0 * Math.PI - 0.001;
                    }
                    
                    double startX = cx + r * Math.Cos(startAngle);
                    double startY = cy + r * Math.Sin(startAngle);
                    double endX = cx + r * Math.Cos(endAngle);
                    double endY = cy + r * Math.Sin(endAngle);
                    
                    int isLargeArc = (pct > 50.0) ? 1 : 0;
                    
                    item.PathData = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "M {0:F2},{1:F2} A {2:F2},{2:F2} 0 {3} 1 {4:F2},{5:F2}",
                        startX, startY, r, isLargeArc, endX, endY);
                        
                    currentAngle = endAngle;
                    CategoryExpenses.Add(item);
                }
            }
        }

        private class CategoryExpenseItemRaw
        {
            public string Name { get; set; }
            public string Color { get; set; }
            public decimal MonthlyCost { get; set; }
        }
    }

    public class CategoryExpenseItem
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public double Percentage { get; set; }
        public string FormattedCostWithPercentage { get; set; }
        public string PathData { get; set; }
    }
}
