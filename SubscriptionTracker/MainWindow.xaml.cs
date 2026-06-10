using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media;
using SubscriptionTracker.ViewModels;
using System;

namespace SubscriptionTracker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            DataContext = vm;
            
            vm.StartTransitionAnimation += Vm_StartTransitionAnimation;
            vm.EndTransitionAnimation += Vm_EndTransitionAnimation;
        }

        private void Vm_StartTransitionAnimation()
        {
            var storyboard = new Storyboard();
            var ease = new CubicEase { EasingMode = EasingMode.EaseIn };

            var scaleX = new DoubleAnimation(1.0, 1.15, TimeSpan.FromMilliseconds(250)) { EasingFunction = ease };
            var scaleY = new DoubleAnimation(1.0, 1.15, TimeSpan.FromMilliseconds(250)) { EasingFunction = ease };
            var opacity = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(200));

            Storyboard.SetTarget(scaleX, MainContent);
            Storyboard.SetTarget(scaleY, MainContent);
            Storyboard.SetTarget(opacity, MainContent);

            Storyboard.SetTargetProperty(scaleX, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            Storyboard.SetTargetProperty(opacity, new PropertyPath("Opacity"));

            storyboard.Children.Add(scaleX);
            storyboard.Children.Add(scaleY);
            storyboard.Children.Add(opacity);

            storyboard.Begin();
        }

        private void Vm_EndTransitionAnimation()
        {
            MainContent.BeginAnimation(UIElement.OpacityProperty, null);
            MainContent.Opacity = 1;
            
            if (MainContent.RenderTransform is ScaleTransform st)
            {
                st.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                st.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}