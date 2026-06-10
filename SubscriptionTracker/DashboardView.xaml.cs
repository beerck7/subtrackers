using System.Windows.Controls;

namespace SubscriptionTracker
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            DataContext = new ViewModels.DashboardViewModel();
        }
    }
}
