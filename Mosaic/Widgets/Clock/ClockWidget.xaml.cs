using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Mosaic.Base;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace Clock
{
    /// <summary>
    /// Interaction logic for ClockWidget.xaml
    /// </summary>
    public partial class ClockWidget : UserControl
    {
        private DispatcherTimer timer;
        private Options optionsWindow;
        private HubWindow hub;
        private Hub hubContent;
        private DispatcherTimer autoLockTimer;
        private bool isLocked;

        public ClockWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            //System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language);
            //System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(E.Language);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick;
            timer.Start();


            autoLockTimer = new DispatcherTimer();
            autoLockTimer.Interval = TimeSpan.FromSeconds(Widget.Settings.AutolockTime);
            autoLockTimer.Tick += AutoLockTimerTick;

            Day.Text = DateTime.Now.ToString("dddd");
            Day.Text = char.ToUpper(Day.Text[0]) + Day.Text.Substring(1);
            Date.Text = DateTime.Now.ToString("MMMM") + " " + DateTime.Now.Day;

            TimerTick(null, EventArgs.Empty);
        }

        void AutoLockTimerTick(object sender, EventArgs e)
        {
            if (!isLocked)
                Lock();
        }

        private System.Drawing.Point lastMousePos;
        void TimerTick(object sender, EventArgs e)
        {
            Time.Text = DateTime.Now.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);

            var power = SystemInformation.PowerStatus;
            if (power.BatteryChargeStatus == BatteryChargeStatus.NoSystemBattery)
                BatteryIcon.Visibility = System.Windows.Visibility.Collapsed;
            else
            {
                BatteryIcon.Visibility = System.Windows.Visibility.Visible;
                var iconNumber = (int)(power.BatteryLifePercent * 10) + 1;
                if (iconNumber >= 10)
                    BatteryIcon.Source = new BitmapImage(new Uri("Resources/batt10.png", UriKind.Relative));
                else
                    BatteryIcon.Source = new BitmapImage(new Uri(string.Format("Resources/batt{0}.png", iconNumber), UriKind.Relative));
            }

            if (Widget.Settings.Autolock)
            {
                var mousePos = System.Windows.Forms.Control.MousePosition;
                if (mousePos == lastMousePos)
                {
                    if (!autoLockTimer.IsEnabled)
                    {
                        autoLockTimer.Start();
                    }
                }
                else
                {
                    lastMousePos = mousePos;
                    autoLockTimer.Stop();
                }
            }
        }

        private void OptionsItemClick(object sender, RoutedEventArgs e)
        {
            if (optionsWindow != null && optionsWindow.IsVisible)
            {
                optionsWindow.Activate();
                return;
            }

            optionsWindow = new Options();
            optionsWindow.UpdateSettings += OptionsWindowUpdateSettings;

            if (E.Language == "he-IL" || E.Language == "ar-SA")
            {
                optionsWindow.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                optionsWindow.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            optionsWindow.ShowDialog();
        }

        void OptionsWindowUpdateSettings(object sender, EventArgs e)
        {
            optionsWindow.UpdateSettings -= OptionsWindowUpdateSettings;

            autoLockTimer.Interval = TimeSpan.FromSeconds(Widget.Settings.AutolockTime);
        }

        public void Unload()
        {
            timer.Tick -= TimerTick;
            timer.Stop();
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Lock();
        }

        internal void Lock()
        {
            if (hub != null && hub.IsVisible)
            {
                hub.Activate();
                return;
            }

            hub = new HubWindow();
            hub.AnimatedOpen = false;
            hub.Topmost = true;
            hub.AllowsTransparency = true;
            hub.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00000000"));
            hubContent = new Hub();
            hubContent.Unlocked += HubContentUnlocked;
            hub.Content = hubContent;
            hub.KeyUp += HubKeyUp;

            if (E.Language == "he-IL" || E.Language == "ar-SA")
            {
                hub.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                hub.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            isLocked = true;

            Share.SendMessage("Mosaic.Widgets", "Music:IsMediaLoaded");

            hub.ShowDialog();
        }

        void HubKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                hub.KeyUp -= HubKeyUp;
                hubContent.Unlock();
            }
        }

        void HubContentUnlocked(object sender, EventArgs e)
        {
            hubContent.Unlocked -= HubContentUnlocked;
            hub.Close();
            isLocked = false;
        }

        public void HubShowMusic()
        {
            if (!isLocked)
                return;
            hubContent.MusicPanel.Visibility = Visibility.Visible;
        }

        public void HubPause()
        {
            if (!isLocked)
                return;
            hubContent.PlayPause.IsChecked = false;
        }

        public void HubPlay()
        {
            if (!isLocked)
                return;
            hubContent.PlayPause.IsChecked = true;
        }

        public void HubUpdateMediaInfo()
        {
            if (!isLocked)
                return;
            hubContent.ChangeMedia();
        }
    }
}
