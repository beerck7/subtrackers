using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace SubscriptionTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using (var db = new Data.AppDbContext())
            {
                db.Database.EnsureCreated();
            }

            // Apply saved theme
            if (!Services.AppSettings.IsDarkTheme)
            {
                try
                {
                    var app = Application.Current;
                    ((SolidColorBrush)app.Resources["AppBackground"]).Color = (Color)ColorConverter.ConvertFromString("#F4F6F9");
                    ((SolidColorBrush)app.Resources["CardBackground"]).Color = (Color)ColorConverter.ConvertFromString("#FFFFFF");
                    ((SolidColorBrush)app.Resources["CardBorder"]).Color = (Color)ColorConverter.ConvertFromString("#E2E8F0");
                    ((SolidColorBrush)app.Resources["InputBackground"]).Color = (Color)ColorConverter.ConvertFromString("#EDF2F7");
                    ((SolidColorBrush)app.Resources["TextPrimary"]).Color = (Color)ColorConverter.ConvertFromString("#1A202C");
                    ((SolidColorBrush)app.Resources["TextSecondary"]).Color = (Color)ColorConverter.ConvertFromString("#718096");
                }
                catch { }
            }
        }
    }
}
