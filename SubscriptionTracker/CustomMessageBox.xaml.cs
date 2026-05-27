using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SubscriptionTracker
{
    public partial class CustomMessageBox : Window
    {
        private MessageBoxResult _result = MessageBoxResult.None;

        public CustomMessageBox()
        {
            InitializeComponent();
        }

        public static MessageBoxResult Show(string messageBoxText, string caption = "SubTrack", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
        {
            var msgBox = new CustomMessageBox();
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                msgBox.Owner = Application.Current.MainWindow;
                msgBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            msgBox.TxtMessage.Text = messageBoxText;
            msgBox.TxtTitle.Text = string.IsNullOrWhiteSpace(caption) ? "SubTrack" : caption;

            // Configure Icon
            switch (icon)
            {
                case MessageBoxImage.Information:
                    msgBox.TxtIcon.Text = "\xE9CE"; // Info
                    msgBox.TxtIcon.Foreground = Application.Current.Resources["PrimaryBrush"] as Brush ?? new SolidColorBrush(Colors.DodgerBlue);
                    break;
                case MessageBoxImage.Warning:
                    msgBox.TxtIcon.Text = "\xE7BA"; // Warning
                    msgBox.TxtIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#eab308"));
                    break;
                case MessageBoxImage.Error:
                    msgBox.TxtIcon.Text = "\xE783"; // Error
                    msgBox.TxtIcon.Foreground = Application.Current.Resources["DangerBrush"] as Brush ?? new SolidColorBrush(Colors.Red);
                    break;
                case MessageBoxImage.Question:
                    msgBox.TxtIcon.Text = "\xE9CE"; // Question/Info
                    msgBox.TxtIcon.Foreground = Application.Current.Resources["PrimaryBrush"] as Brush ?? new SolidColorBrush(Colors.DodgerBlue);
                    break;
                default:
                    msgBox.TxtIcon.Text = "\xE73E"; // Checkmark as default success
                    msgBox.TxtIcon.Foreground = Application.Current.Resources["SuccessBrush"] as Brush ?? new SolidColorBrush(Colors.LimeGreen);
                    break;
            }

            // Configure Buttons
            if (button == MessageBoxButton.YesNo)
            {
                msgBox.BtnOk.Visibility = Visibility.Collapsed;
                msgBox.BtnYes.Visibility = Visibility.Visible;
                msgBox.BtnNo.Visibility = Visibility.Visible;
            }
            else
            {
                msgBox.BtnOk.Visibility = Visibility.Visible;
                msgBox.BtnYes.Visibility = Visibility.Collapsed;
                msgBox.BtnNo.Visibility = Visibility.Collapsed;
            }

            // Show modally
            msgBox.ShowDialog();
            return msgBox._result;
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.Cancel;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.OK;
            Close();
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.Yes;
            Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.No;
            Close();
        }
    }
}
