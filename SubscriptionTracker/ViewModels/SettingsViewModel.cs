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
            _userName = SessionManager.CurrentUser?.Username ?? AppSettings.UserName;
            _userEmail = SessionManager.CurrentUser?.Email ?? AppSettings.UserEmail;
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
                UserName = SessionManager.CurrentUser?.Username ?? AppSettings.UserName;
                UserEmail = SessionManager.CurrentUser?.Email ?? AppSettings.UserEmail;
            }
        }

        [RelayCommand]
        private async Task SaveProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(UserEmail))
            {
                CustomMessageBox.Show("Nazwa i e-mail nie mogą być puste.", "Błąd zapisu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var currentUser = SessionManager.CurrentUser;
            if (currentUser != null)
            {
                currentUser.Username = UserName;
                currentUser.Email = UserEmail;

                var ds = new DataService();
                await ds.UpdateUserAsync(currentUser);
            }

            IsEditing = false;

            CustomMessageBox.Show("Profil użytkownika został pomyślnie zaktualizowany!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
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
            CustomMessageBox.Show($"Powiadomienia o płatnościach zostały {status}.", "Ustawienia powiadomień", MessageBoxButton.OK, MessageBoxImage.Information);
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
            var result = CustomMessageBox.Show(
                "Czy na pewno chcesz usunąć wszystkie subskrypcje i kategorie z bazy danych?\nTej operacji nie można cofnąć!",
                "Potwierdzenie usunięcia danych",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var userId = SessionManager.CurrentUser?.Id ?? 0;
                using (var db = new AppDbContext())
                {
                    var userSubs = await db.Subscriptions.Where(s => s.UserId == userId).ToListAsync();
                    db.Subscriptions.RemoveRange(userSubs);

                    var userCats = await db.Categories.Where(c => c.UserId == userId).ToListAsync();
                    db.Categories.RemoveRange(userCats);
                    
                    // Re-seed default categories specifically for this user
                    db.Categories.Add(new Models.Category { Name = "Streaming", Color = "#22c55e", UserId = userId });
                    db.Categories.Add(new Models.Category { Name = "Muzyka", Color = "#f43f5e", UserId = userId });
                    db.Categories.Add(new Models.Category { Name = "Oprogramowanie", Color = "#3b82f6", UserId = userId });
                    db.Categories.Add(new Models.Category { Name = "Gry", Color = "#eab308", UserId = userId });
                    db.Categories.Add(new Models.Category { Name = "Inne", Color = "#a855f7", UserId = userId });
                    
                    await db.SaveChangesAsync();
                }

                CustomMessageBox.Show("Wszystkie Twoje dane zostały wyczyszczone. Przywrócono domyślne kategorie.", "Wyczyszczono dane", MessageBoxButton.OK, MessageBoxImage.Information);
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

        [RelayCommand]
        private async Task ExportReportAsync()
        {
            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Pliki HTML (*.html)|*.html",
                FileName = $"Raport_SubTrack_{DateTime.Now:yyyyMMdd}.html",
                Title = "Wybierz lokalizację zapisu raportu"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var ds = new DataService();
                    var subs = await ds.GetSubscriptionsAsync();
                    var logs = await ds.GetPaymentLogsAsync();

                    // Calculate stats
                    decimal monthlyTotal = 0;
                    foreach (var s in subs.Where(x => x.Status == "Aktywna" || string.IsNullOrEmpty(x.Status)))
                    {
                        decimal cost = s.IsShared && s.NumberOfMembers > 1 ? s.SplitPrice : s.Price;
                        if (s.Cycle == "Rocznie")
                            monthlyTotal += cost / 12;
                        else
                            monthlyTotal += cost;
                    }
                    decimal yearlyTotal = monthlyTotal * 12;
                    int activeCount = subs.Count(x => x.Status == "Aktywna" || string.IsNullOrEmpty(x.Status));

                    // Build Subscriptions Rows
                    var subRowsHtml = new System.Text.StringBuilder();
                    foreach (var s in subs)
                    {
                        string catName = s.Category?.Name ?? "Brak";
                        string statusBadge = s.Status == "Aktywna" ? "<span class=\"badge active\">Aktywna</span>" : "<span class=\"badge inactive\">Nieaktywna</span>";
                        string splitDetails = s.IsShared ? $"Dzielone z: {s.SharedWith} ({s.NumberOfMembers} os.)" : "Brak współdzielenia";
                        string yourShareText = s.IsShared ? $"{s.SplitPrice:0.00} PLN" : $"{s.Price:0.00} PLN";

                        subRowsHtml.AppendLine("<tr>");
                        subRowsHtml.AppendLine($"  <td><strong>{System.Net.WebUtility.HtmlEncode(s.Name)}</strong></td>");
                        subRowsHtml.AppendLine($"  <td>{System.Net.WebUtility.HtmlEncode(catName)}</td>");
                        subRowsHtml.AppendLine($"  <td>{s.Price:0.00} PLN</td>");
                        subRowsHtml.AppendLine($"  <td>{System.Net.WebUtility.HtmlEncode(splitDetails)}</td>");
                        subRowsHtml.AppendLine($"  <td><strong style=\"color:#22c55e;\">{yourShareText}</strong></td>");
                        subRowsHtml.AppendLine($"  <td>{System.Net.WebUtility.HtmlEncode(s.Cycle)}</td>");
                        subRowsHtml.AppendLine($"  <td>{s.NextPaymentDate:dd.MM.yyyy}</td>");
                        subRowsHtml.AppendLine($"  <td>{statusBadge}</td>");
                        subRowsHtml.AppendLine("</tr>");
                    }
                    if (subs.Count == 0)
                    {
                        subRowsHtml.AppendLine("<tr><td colspan=\"8\" style=\"text-align:center;color:#64748b;\">Brak zarejestrowanych subskrypcji</td></tr>");
                    }

                    // Build Payment Log Rows
                    var logRowsHtml = new System.Text.StringBuilder();
                    foreach (var l in logs)
                    {
                        string subName = l.Subscription?.Name ?? "Subskrypcja usunięta";
                        logRowsHtml.AppendLine("<tr>");
                        logRowsHtml.AppendLine($"  <td><strong>{System.Net.WebUtility.HtmlEncode(subName)}</strong></td>");
                        logRowsHtml.AppendLine($"  <td>{l.PaymentDate:dd.MM.yyyy HH:mm}</td>");
                        logRowsHtml.AppendLine($"  <td style=\"color:#22c55e;font-weight:bold;\">{l.AmountPaid:0.00} PLN</td>");
                        logRowsHtml.AppendLine($"  <td>{System.Net.WebUtility.HtmlEncode(l.Note)}</td>");
                        logRowsHtml.AppendLine("</tr>");
                    }
                    if (logs.Count == 0)
                    {
                        logRowsHtml.AppendLine("<tr><td colspan=\"4\" style=\"text-align:center;color:#64748b;\">Brak historii płatności</td></tr>");
                    }

                    // Load report template and swap properties
                    string template = GetHtmlReportTemplate();
                    template = template.Replace("@Username", System.Net.WebUtility.HtmlEncode(SessionManager.CurrentUser?.Username ?? "Użytkownik"))
                                       .Replace("@UserEmail", System.Net.WebUtility.HtmlEncode(SessionManager.CurrentUser?.Email ?? "brak@email.com"))
                                       .Replace("@GeneratedTime", DateTime.Now.ToString("dd.MM.yyyy HH:mm"))
                                       .Replace("@MonthlyCosts", $"{monthlyTotal:0.00}")
                                       .Replace("@YearlyCosts", $"{yearlyTotal:0.00}")
                                       .Replace("@ActiveCount", activeCount.ToString())
                                       .Replace("@SubscriptionsRows", subRowsHtml.ToString())
                                       .Replace("@PaymentLogsRows", logRowsHtml.ToString())
                                       .Replace("@CurrentYear", DateTime.Now.Year.ToString());

                    await System.IO.File.WriteAllTextAsync(sfd.FileName, template, System.Text.Encoding.UTF8);

                    CustomMessageBox.Show($"Raport finansowy został pomyślnie wyeksportowany do:\n{sfd.FileName}", "Raport wyeksportowany", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Wystąpił błąd podczas eksportowania raportu:\n{ex.Message}", "Błąd eksportu", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string GetHtmlReportTemplate()
        {
            return @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <title>SubTrack Premium Financial Report</title>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; background-color: #0c0d12; color: #f8fafc; margin: 0; padding: 40px; }
        .container { max-width: 900px; margin: 0 auto; background: #16171e; border: 1px solid #272733; border-radius: 12px; padding: 30px; box-shadow: 0 10px 30px rgba(0,0,0,0.5); }
        .header { display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #272733; padding-bottom: 20px; margin-bottom: 30px; }
        .header h1 { margin: 0; font-size: 28px; color: #22c55e; letter-spacing: 0.5px; }
        .meta { color: #94a3b8; font-size: 14px; text-align: right; }
        .meta p { margin: 4px 0; }
        .stats-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; margin-bottom: 30px; }
        .stat-card { background: #1f2029; border: 1px solid #272733; border-radius: 8px; padding: 20px; text-align: center; }
        .stat-card h3 { margin: 0 0 10px 0; color: #94a3b8; font-size: 14px; text-transform: uppercase; }
        .stat-card .value { font-size: 24px; font-weight: bold; color: #ffffff; }
        .stat-card .value.highlight { color: #22c55e; }
        .section-title { font-size: 20px; font-weight: 600; color: #ffffff; margin: 30px 0 15px 0; border-left: 4px solid #3b82f6; padding-left: 10px; }
        table { width: 100%; border-collapse: collapse; margin-bottom: 20px; background: #1c1d26; border-radius: 8px; overflow: hidden; }
        th { background: #22232e; text-align: left; padding: 12px 15px; font-size: 13px; font-weight: 600; color: #94a3b8; border-bottom: 1px solid #272733; }
        td { padding: 12px 15px; font-size: 14px; border-bottom: 1px solid #1f2029; color: #e2e8f0; }
        tr:last-child td { border-bottom: none; }
        .badge { display: inline-block; padding: 4px 8px; border-radius: 4px; font-size: 11px; font-weight: bold; }
        .badge.active { background: rgba(34, 197, 94, 0.2); color: #22c55e; }
        .badge.inactive { background: rgba(239, 68, 68, 0.2); color: #ef4444; }
        .badge.shared { background: rgba(59, 130, 246, 0.2); color: #3b82f6; }
        .footer { text-align: center; color: #64748b; font-size: 12px; margin-top: 40px; border-top: 1px solid #272733; padding-top: 20px; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div>
                <h1 style=""color: #22c55e;"">SubTrack Report</h1>
                <p style=""margin: 5px 0 0 0; color: #94a3b8;"">Financial Subscription & Expense Statement</p>
            </div>
            <div class=""meta"">
                <p>User: <strong>@Username</strong> (@UserEmail)</p>
                <p>Generated: <strong>@GeneratedTime</strong></p>
            </div>
        </div>
        
        <div class=""stats-grid"">
            <div class=""stat-card"">
                <h3>Monthly Expenses</h3>
                <div class=""value highlight"">@MonthlyCosts PLN</div>
            </div>
            <div class=""stat-card"">
                <h3>Yearly Projected</h3>
                <div class=""value"">@YearlyCosts PLN</div>
            </div>
            <div class=""stat-card"">
                <h3>Active Services</h3>
                <div class=""value"">@ActiveCount</div>
            </div>
        </div>
        
        <div class=""section-title"">Subscriptions & Cost Splits</div>
        <table>
            <thead>
                <tr>
                    <th>Service</th>
                    <th>Category</th>
                    <th>Full Price</th>
                    <th>Split Details</th>
                    <th>Your Share</th>
                    <th>Cycle</th>
                    <th>Next Payment</th>
                    <th>Status</th>
                </tr>
            </thead>
            <tbody>
                @SubscriptionsRows
            </tbody>
        </table>
        
        <div class=""section-title"">Recent Payment Ledger Activity</div>
        <table>
            <thead>
                <tr>
                    <th>Service</th>
                    <th>Date Paid</th>
                    <th>Amount Paid</th>
                    <th>System Note</th>
                </tr>
            </thead>
            <tbody>
                @PaymentLogsRows
            </tbody>
        </table>
        
        <div class=""footer"">
            <p>Generated dynamically by SubTrack WPF. &copy; @CurrentYear SubTrack Team.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
