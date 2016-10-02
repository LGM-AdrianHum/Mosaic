using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mosaic.Base;

namespace Gmail
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class Options : Window
    {
        public event EventHandler UpdateSettings;

        public Options()
        {
            InitializeComponent();
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            this.Top = 0;
            this.Left = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            BuildTag.Text = version + ".beta." + fileInfo.LastWriteTimeUtc.ToString("yyMMdd-HHmm");

            UsernameBox.Text = Widget.Settings.Username;
            PassBox.Password = Widget.Settings.Password;

            var s = (Storyboard)Resources["LoadAnim"];
            ((DoubleAnimation)s.Children[0]).From = SystemParameters.PrimaryScreenWidth;
            s.Begin(this);
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            CloseWindow();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void ApplyButtonClick(object sender, RoutedEventArgs e)
        {
            ApplySettings();
        }

        private void SiteLinkMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WinAPI.ShellExecute(IntPtr.Zero, "open", "http://mosaicproject.codeplex.com", string.Empty, string.Empty, 0);
        }

        private void ApplySettings()
        {
            Widget.Settings.Username = UsernameBox.Text;
            Widget.Settings.Password = PassBox.Password;

            Widget.Settings.Save(E.WidgetsRoot + "\\Gmail\\Gmail.config");
            if (UpdateSettings != null)
            {
                UpdateSettings(null, EventArgs.Empty);
            }
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseWindow();
        }


        private void CloseWindow()
        {
            var s = (Storyboard)Resources["UnloadAnim"];
            ((DoubleAnimation)s.Children[0]).To = -SystemParameters.PrimaryScreenWidth;
            s.Begin(this);
        }

        private void UnloadAnimCompleted(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PasswordBoxTextChanged(object sender, RoutedEventArgs e)
        {
            if (ShowPassCheckBox.IsChecked == false)
                VisiblePassBox.Text = PassBox.Password;
        }

        private void VisiblePassBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ShowPassCheckBox.IsChecked == true)
                PassBox.Password = VisiblePassBox.Text;
        }

        private void ShowPassCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            VisiblePassBox.Visibility = Visibility.Visible;
        }

        private void ShowPassCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            VisiblePassBox.Visibility = Visibility.Collapsed;
        }
    }
}
