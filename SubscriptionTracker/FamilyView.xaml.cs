using System.Windows.Controls;
using SubscriptionTracker.ViewModels;

namespace SubscriptionTracker
{
    public partial class FamilyView : UserControl
    {
        public FamilyView()
        {
            InitializeComponent();
            DataContext = new FamilyViewModel();
        }
    }
}
