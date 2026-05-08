using System.Windows;
using SubscriptionTracker.ViewModels;

namespace SubscriptionTracker
{
    public partial class AddSubscriptionWindow : Window
    {
        public AddSubscriptionWindow()
        {
            InitializeComponent();
            DataContext = new AddSubscriptionViewModel();
        }
    }
}
