using System.Windows.Controls;
using SubscriptionTracker.ViewModels;

namespace SubscriptionTracker
{
    public partial class CategoriesView : UserControl
    {
        public CategoriesView()
        {
            InitializeComponent();
            DataContext = new CategoriesViewModel();
        }
    }
}
