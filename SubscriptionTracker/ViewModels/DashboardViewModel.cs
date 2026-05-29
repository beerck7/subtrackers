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

        [ObservableProperty]
        private ObservableCollection<PaymentLogViewModel> _recentPayments;

        [ObservableProperty]
        private ObservableCollection<OrbitalPlanetViewModel> _orbitalPlanets;

        [ObservableProperty]
        private OrbitalPlanetViewModel _selectedPlanet;

        [ObservableProperty]
        private int _projectionMonths = 1;

        [ObservableProperty]
        private string _projectedTotalCostText = "0,00 PLN";

        public DashboardViewModel()
        {
            _dataService = new DataService();
            UpcomingPayments = new ObservableCollection<SubscriptionItemViewModel>();
            CategoryExpenses = new ObservableCollection<CategoryExpenseItem>();
            RecentPayments = new ObservableCollection<PaymentLogViewModel>();
            OrbitalPlanets = new ObservableCollection<OrbitalPlanetViewModel>();

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
                decimal cost = sub.IsShared && sub.NumberOfMembers > 1 ? sub.SplitPrice : sub.Price;
                if (sub.Cycle == "Rocznie")
                {
                    _totalMonthlyCostCalculated += cost / 12;
                }
                else
                {
                    _totalMonthlyCostCalculated += cost;
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

            // Generate Orbital Planets (Solar Galaxy)
            OrbitalPlanets.Clear();
            var sortedActive = activeSubs.OrderBy(s => s.NextPaymentDate).ToList();
            int subCount = sortedActive.Count;
            
            for (int i = 0; i < subCount; i++)
            {
                var sub = sortedActive[i];
                decimal cost = sub.IsShared && sub.NumberOfMembers > 1 ? sub.SplitPrice : sub.Price;
                decimal monthlyEquivalent = sub.Cycle == "Rocznie" ? cost / 12 : cost;
                
                // Days left until payment
                int daysLeft = (sub.NextPaymentDate - now).Days;
                if (daysLeft < 0) daysLeft = 0;
                
                // 1. Calculate Orbit Radius (High Urgency = Close, Low Urgency = Far)
                double radius;
                if (daysLeft <= 7)
                {
                    radius = 50; // Inner orbit
                }
                else if (daysLeft <= 21)
                {
                    radius = 95; // Middle orbit
                }
                else
                {
                    radius = 140; // Outer orbit
                }
                
                // 2. Distribute planets angularly to avoid overlaps
                double angle = (2.0 * Math.PI / Math.Max(subCount, 1)) * i;
                
                // Center of Canvas is (200, 160)
                double cx = 200;
                double cy = 160;
                double size = 24 + (double)(Math.Min(monthlyEquivalent, 100M) / 100M) * 24;
                
                double x = cx + radius * Math.Cos(angle) - (size / 2.0); // Offset by half size
                double y = cy + radius * Math.Sin(angle) - (size / 2.0);
                
                var planet = new OrbitalPlanetViewModel
                {
                    Model = sub,
                    Name = sub.Name,
                    Color = sub.Category?.Color ?? "#808080",
                    Price = sub.Price,
                    SplitPrice = sub.IsShared && sub.NumberOfMembers > 1 ? sub.SplitPrice : sub.Price,
                    IsShared = sub.IsShared,
                    Cycle = sub.Cycle,
                    DaysLeft = daysLeft,
                    X = x,
                    Y = y,
                    Size = size,
                    ToolTipText = $"{sub.Name} ({sub.Category?.Name ?? "Brak"})\nKoszt: {cost:0.00} PLN\nPozostało dni: {daysLeft}"
                };
                
                OrbitalPlanets.Add(planet);
            }
            
            // Set first planet selected by default
            SelectedPlanet = OrbitalPlanets.FirstOrDefault();
            if (SelectedPlanet != null)
            {
                SelectedPlanet.IsHighlighted = true;
            }
            
            CalculateFutureProjection();

            // Load transaction payment logs history
            var logs = await _dataService.GetPaymentLogsAsync();
            var recent = logs.Take(5).Select(l => new PaymentLogViewModel
            {
                SubscriptionName = l.Subscription?.Name ?? "Subskrypcja usunięta",
                IconColor = l.Subscription?.Category?.Color ?? "#808080",
                Initials = string.IsNullOrWhiteSpace(l.Subscription?.Name) ? "?" : l.Subscription.Name.Substring(0, 1).ToUpper(),
                PaymentDateFormatted = l.PaymentDate.ToString("dd.MM.yyyy HH:mm"),
                FormattedAmount = $"{l.AmountPaid:0.00} PLN",
                Note = l.Note
            }).ToList();

            RecentPayments.Clear();
            foreach (var r in recent)
            {
                RecentPayments.Add(r);
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
                        decimal cost = sub.IsShared && sub.NumberOfMembers > 1 ? sub.SplitPrice : sub.Price;
                        if (sub.Cycle == "Rocznie")
                        {
                            catMonthly += cost / 12;
                        }
                        else
                        {
                            catMonthly += cost;
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

        [RelayCommand]
        private void SelectPlanet(OrbitalPlanetViewModel planet)
        {
            if (planet == null) return;
            
            foreach (var p in OrbitalPlanets)
            {
                p.IsHighlighted = (p == planet);
            }
            
            SelectedPlanet = planet;
        }

        [RelayCommand]
        private async Task LogPlanetPaymentAsync(OrbitalPlanetViewModel planet)
        {
            if (planet == null) return;
            var success = await _dataService.LogSubscriptionPaymentAsync(planet.Model.Id, null);
            if (success)
            {
                CustomMessageBox.Show($"Płatność za subskrypcję {planet.Name} została pomyślnie zarejestrowana!", "Płatność Zarejestrowana", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                await LoadDashboardDataAsync();
            }
        }

        partial void OnProjectionMonthsChanged(int value)
        {
            CalculateFutureProjection();
        }

        private void CalculateFutureProjection()
        {
            if (OrbitalPlanets == null || OrbitalPlanets.Count == 0)
            {
                ProjectedTotalCostText = "0,00 PLN";
                return;
            }
            
            decimal totalProjected = 0;
            var now = DateTime.Now;
            var futureDate = now.AddMonths(ProjectionMonths);
            
            foreach (var planet in OrbitalPlanets)
            {
                var sub = planet.Model;
                decimal cost = sub.IsShared && sub.NumberOfMembers > 1 ? sub.SplitPrice : sub.Price;
                
                var billingCursor = sub.NextPaymentDate;
                int timesBilled = 0;
                
                while (billingCursor <= futureDate)
                {
                    timesBilled++;
                    if (sub.Cycle == "Rocznie")
                    {
                        billingCursor = billingCursor.AddYears(1);
                    }
                    else // Miesięcznie
                    {
                        billingCursor = billingCursor.AddMonths(1);
                    }
                }
                
                if (timesBilled == 0 && sub.NextPaymentDate <= futureDate)
                {
                    timesBilled = 1;
                }
                
                totalProjected += cost * timesBilled;
            }
            
            ProjectedTotalCostText = $"{totalProjected:0.00} {Services.AppSettings.DefaultCurrency}";
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

    public class PaymentLogViewModel
    {
        public string SubscriptionName { get; set; }
        public string IconColor { get; set; }
        public string Initials { get; set; }
        public string PaymentDateFormatted { get; set; }
        public string FormattedAmount { get; set; }
        public string Note { get; set; }
    }

    public partial class OrbitalPlanetViewModel : ObservableObject
    {
        public Subscription Model { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public decimal Price { get; set; }
        public decimal SplitPrice { get; set; }
        public bool IsShared { get; set; }
        public string Cycle { get; set; }
        public int DaysLeft { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Size { get; set; }
        public string ToolTipText { get; set; }
        public string Initials => string.IsNullOrWhiteSpace(Name) ? "?" : Name.Substring(0, 1).ToUpper();

        [ObservableProperty]
        private bool _isHighlighted;
    }
}
