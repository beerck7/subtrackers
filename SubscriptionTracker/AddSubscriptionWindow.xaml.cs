using System.Windows;
using SubscriptionTracker.ViewModels;

namespace SubscriptionTracker
{
    public partial class AddSubscriptionWindow : Window
    {
        public AddSubscriptionWindow() : this(null)
        {
        }

        public AddSubscriptionWindow(Models.Subscription subscriptionToEdit)
        {
            InitializeComponent();
            DataContext = new AddSubscriptionViewModel(subscriptionToEdit);
        }
    }
}
