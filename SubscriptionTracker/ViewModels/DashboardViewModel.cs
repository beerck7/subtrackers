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
        private string _monthlyCostsText = "0,00";

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
            MonthlyCostsText = $"{totalMonthly:0.00} {Services.AppSettings.DefaultCurrency}";

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
                var categoryList = new System.Collections.Generic.List<CategoryExpenseItem>();
                
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
                        categoryList.Add(new CategoryExpenseItem
                        {
                            Name = cat.Name,
                            Color = cat.Color ?? "#808080",
                            Percentage = (double)pct,
                            FormattedCostWithPercentage = $"{catMonthly:0.00} {Services.AppSettings.DefaultCurrency} ({pct:0}%)",
                            PathData = ""
                        });
                    }
                }
                
                // Calculate dynamic arcs for our beautiful premium donut chart
                double currentAngle = -Math.PI / 2.0; // Start at the top (-90 degrees)
                foreach (var item in categoryList)
                {
                    double sweepAngle = (item.Percentage / 100.0) * 2.0 * Math.PI;
                    
                    double cx = 75.0;
                    double cy = 75.0;
                    double r = 60.0;
                    
                    double startAngle = currentAngle;
                    double endAngle = currentAngle + sweepAngle;
                    
                    if (item.Percentage >= 99.9)
                    {
                        endAngle = startAngle + 2.0 * Math.PI - 0.001;
                    }
                    
                    double startX = cx + r * Math.Cos(startAngle);
                    double startY = cy + r * Math.Sin(startAngle);
                    double endX = cx + r * Math.Cos(endAngle);
                    double endY = cy + r * Math.Sin(endAngle);
                    
                    int isLargeArc = (item.Percentage > 50.0) ? 1 : 0;
                    
                    item.PathData = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "M {0:F2},{1:F2} A {2:F2},{2:F2} 0 {3} 1 {4:F2},{5:F2}",
                        startX, startY, r, isLargeArc, endX, endY);
                        
                    currentAngle = endAngle;
                    CategoryExpenses.Add(item);
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
        public string PathData { get; set; }
    }
}
