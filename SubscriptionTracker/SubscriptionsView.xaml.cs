using System.Windows.Controls;
using SubscriptionTracker.ViewModels;

namespace SubscriptionTracker
{
    public partial class SubscriptionsView : UserControl
    {
        public SubscriptionsView()
        {
            InitializeComponent();
            DataContext = new SubscriptionsViewModel();
        }
    }
}
