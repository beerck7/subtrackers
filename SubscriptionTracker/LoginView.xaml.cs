using SubscriptionTracker.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SubscriptionTracker
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = PasswordInput.Password;
                await vm.SubmitCommand.ExecuteAsync(null);
                
                if (string.IsNullOrEmpty(vm.Password))
                {
                    PasswordInput.Clear();
                }
            }
        }
    }
}
