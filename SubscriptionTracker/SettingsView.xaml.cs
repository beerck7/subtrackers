using System.Windows.Controls;
using SubscriptionTracker.ViewModels;

namespace SubscriptionTracker
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
        }
    }
}
