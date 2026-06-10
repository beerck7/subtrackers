using SubscriptionTracker.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Media.Animation;

namespace SubscriptionTracker
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            DataContextChanged += LoginView_DataContextChanged;
        }

        private void LoginView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is LoginViewModel oldVm)
                oldVm.LoginSuccessfulAnimation -= Vm_LoginSuccessfulAnimation;
            
            if (e.NewValue is LoginViewModel newVm)
                newVm.LoginSuccessfulAnimation += Vm_LoginSuccessfulAnimation;
        }

        private void Vm_LoginSuccessfulAnimation()
        {
            var storyboard = new Storyboard();
            var ease = new CubicEase { EasingMode = EasingMode.EaseIn };

            var scaleX = new DoubleAnimation(1.0, 1.25, TimeSpan.FromMilliseconds(350)) { EasingFunction = ease };
            var scaleY = new DoubleAnimation(1.0, 1.25, TimeSpan.FromMilliseconds(350)) { EasingFunction = ease };
            var opacity = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(250)) { BeginTime = TimeSpan.FromMilliseconds(100) };

            Storyboard.SetTarget(scaleX, LoginCard);
            Storyboard.SetTarget(scaleY, LoginCard);
            Storyboard.SetTarget(opacity, LoginCard);

            Storyboard.SetTargetProperty(scaleX, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            Storyboard.SetTargetProperty(opacity, new PropertyPath("Opacity"));

            storyboard.Children.Add(scaleX);
            storyboard.Children.Add(scaleY);
            storyboard.Children.Add(opacity);

            storyboard.Begin();
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
