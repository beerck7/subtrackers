using System;
using System.IO;

namespace SubscriptionTracker.Services
{
    public static class AppSettings
    {
        private static readonly string SettingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");

        public static string UserName { get; set; } = "Norbert Student";
        public static string UserEmail { get; set; } = "z.norbert0104@gmail.com";
        public static bool IsDarkTheme { get; set; } = true;
        public static bool AreNotificationsEnabled { get; set; } = true;
        public static string DefaultCurrency { get; set; } = "PLN";

        static AppSettings()
        {
            Load();
        }

        public static void Save()
        {
            try
            {
                var content = $"{UserName}\n{UserEmail}\n{IsDarkTheme}\n{AreNotificationsEnabled}\n{DefaultCurrency}";
                File.WriteAllText(SettingsFile, content);
            }
            catch { }
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var lines = File.ReadAllLines(SettingsFile);
                    if (lines.Length >= 5)
                    {
                        UserName = lines[0];
                        UserEmail = lines[1];
                        IsDarkTheme = bool.Parse(lines[2]);
                        AreNotificationsEnabled = bool.Parse(lines[3]);
                        DefaultCurrency = lines[4];
                    }
                }
            }
            catch { }
        }
    }
}
