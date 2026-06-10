using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using System.Windows.Media;
using System.Windows.Controls;

namespace SubscriptionTracker
{
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine(e.Exception.ToString());
                sb.AppendLine("\n--- BORDER AUDIT ---");
                
                var mainWin = Application.Current.MainWindow;
                if (mainWin != null)
                {
                    AuditBorders(mainWin, sb);
                }
                else
                {
                    sb.AppendLine("MainWindow is null.");
                }
                
                System.IO.File.WriteAllText("crash_log.txt", sb.ToString());
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("crash_log.txt", "Audit failed: " + ex.ToString() + "\nOriginal exception: " + e.Exception.ToString());
            }
        }

        private void AuditBorders(DependencyObject obj, System.Text.StringBuilder sb)
        {
            if (obj == null) return;
            
            if (obj is Border border)
            {
                try
                {
                    var localVal = border.ReadLocalValue(Border.BackgroundProperty);
                    object effVal = "Exception on get";
                    try { effVal = border.Background; } catch (Exception ex) { effVal = "Get failed: " + ex.Message; }
                    sb.AppendLine($"Border Name: '{border.Name}', Parent: '{VisualTreeHelper.GetParent(border)?.GetType().Name}', Local Background: '{localVal}', Effective Background: '{effVal}'");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"Error auditing a border: {ex.Message}");
                }
            }
            
            try
            {
                int count = VisualTreeHelper.GetChildrenCount(obj);
                for (int i = 0; i < count; i++)
                {
                    AuditBorders(VisualTreeHelper.GetChild(obj, i), sb);
                }
            }
            catch { }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using (var db = new Data.AppDbContext())
            {
                db.Database.EnsureCreated();
                try
                {
                    db.Database.ExecuteSqlRaw(@"
                        CREATE TABLE IF NOT EXISTS FamilyConnections (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            SenderUserId INTEGER NOT NULL,
                            ReceiverUserId INTEGER NOT NULL,
                            IsAccepted INTEGER NOT NULL DEFAULT 0,
                            Relationship TEXT NULL,
                            FOREIGN KEY(SenderUserId) REFERENCES Users(Id) ON DELETE CASCADE,
                            FOREIGN KEY(ReceiverUserId) REFERENCES Users(Id) ON DELETE CASCADE
                        );
                    ");
                }
                catch { }
                try
                {
                    db.Database.ExecuteSqlRaw("ALTER TABLE Subscriptions ADD COLUMN Color TEXT NULL;");
                }
                catch { }
                Services.DataSeeder.SeedData(db);
            }


            if (!Services.AppSettings.IsDarkTheme)
            {
                try
                {
                    var app = Application.Current;
                    app.Resources["AppBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F4F6F9"));
                    app.Resources["CardBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                    app.Resources["CardBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
                    app.Resources["InputBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EDF2F7"));
                    app.Resources["TextPrimary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A202C"));
                    app.Resources["TextSecondary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#718096"));
                }
                catch { }
            }
        }
    }
}
