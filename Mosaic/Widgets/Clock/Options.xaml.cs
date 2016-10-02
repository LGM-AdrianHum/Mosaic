using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Mosaic.Base;

namespace Clock
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

            AutoLockCheckBox.IsChecked = Widget.Settings.Autolock;
            LockIntervalSlider.Value = Widget.Settings.AutolockTime / 60;
            LockBgBox.Text = Widget.Settings.LockScreenBg;

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

        private void SiteLinkMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WinAPI.ShellExecute(IntPtr.Zero, "open", "http://mosaicproject.codeplex.com", string.Empty, string.Empty, 0);
        }

        private void ApplySettings()
        {
            Widget.Settings.Autolock = AutoLockCheckBox.IsChecked == true;
            Widget.Settings.LockScreenBg = LockBgBox.Text;
            Widget.Settings.AutolockTime = (int)LockIntervalSlider.Value * 60;
            Widget.Settings.Save(E.WidgetsRoot + "\\Clock\\Clock.config");
            if (UpdateSettings != null)
            {
                UpdateSettings(null, EventArgs.Empty);
            }
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseWindow();
        }

        private void LockIntervalSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LockIntervalSlider.Value < 60)
            {
                LockIntervalValueTextBlock.Text = LockIntervalSlider.Value + " " + Properties.Resources.OptionsIntervalMinutes;
            }
            else if (LockIntervalSlider.Value == 60)
            {
                LockIntervalValueTextBlock.Text = 1 + " " + Properties.Resources.OptionsIntervalHours;
            }
            else
            {
                LockIntervalValueTextBlock.Text = string.Format("{0} {1} {2} {3}", Math.Truncate(LockIntervalSlider.Value / 60), Properties.Resources.OptionsIntervalHours,
                    Math.Abs(Math.IEEERemainder(LockIntervalSlider.Value, 60)), Properties.Resources.OptionsIntervalMinutes);
            }
        }

        private void LockBgChangeButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Images | *.png;*.jpg;*.bmp";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LockBgBox.Text = dialog.FileName;
            }
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
    }
}
