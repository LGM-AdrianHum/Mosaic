using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using Mosaic.Base;

namespace Clock
{
    /// <summary>
    /// Interaction logic for Hub.xaml
    /// </summary>
    public partial class Hub : UserControl
    {
        public event EventHandler Unlocked;
        private DispatcherTimer timer;
        private Random random;

        public Hub()
        {
            InitializeComponent();
        }

        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            ChangeMedia();

            if (Share.SharedStrings.ContainsKey("Music_PlaybackState") && Share.SharedStrings["Music_PlaybackState"] == "playing")
            {
                PlayPause.IsChecked = true;
            }

            string lockBgPath;
            if (string.IsNullOrEmpty(Widget.Settings.LockScreenBg) || !File.Exists(Widget.Settings.LockScreenBg))
            {
                var wpReg = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
                lockBgPath = wpReg.GetValue("WallPaper").ToString();
                wpReg.Close();

                if (string.IsNullOrEmpty(lockBgPath) || !File.Exists(lockBgPath))
                {
                    random = new Random(Environment.TickCount);
                    string[] files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\Web\\Wallpaper", "*.jpg", SearchOption.AllDirectories);
                    lockBgPath = files[random.Next(0, files.Length)];
                }
            }
            else
            {
                lockBgPath = Widget.Settings.LockScreenBg;
            }

            var bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.UriSource = new Uri(lockBgPath);
            bi.EndInit();
            ////Windows 7
            //if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
            //{
            //    var wpReg = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\Desktop\\General\\", false);
            //    wallpaperPath = wpReg.GetValue("WallpaperSource").ToString();
            //    wpReg.Close();
            //}
            //else
            //{
            //    //Windows XP
            //    var wpReg = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
            //    wallpaperPath = wpReg.GetValue("Wallpaper").ToString();
            //    wpReg.Close();
            //}

            LockScreenBg.Source = bi;// new BitmapImage(new Uri(wallpaperPath));
            Day.Text = DateTime.Now.ToString("dddd");
            Day.Text = char.ToUpper(Day.Text[0]) + Day.Text.Substring(1);
            Month.Text = DateTime.Now.ToString("MMMM") + " " + DateTime.Now.Day;
            Time.Text = DateTime.Now.ToShortTimeString();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(TimerTick);
            timer.Start();
            Translate.Y = -SystemParameters.PrimaryScreenHeight;

            var s = (Storyboard)Resources["MoveBackAnim"];
            s.Begin();

            if (Share.SharedStrings.ContainsKey("Today_Title"))
            {
                CalendarPanel.Visibility = Visibility.Visible;
                CalendarTitle.Text = Share.SharedStrings["Today_Title"];
            }

            if (Share.SharedStrings.ContainsKey("Today_Description") && !string.IsNullOrEmpty(Share.SharedStrings["Today_Description"]))
            {
                CalendarDescription.Visibility = Visibility.Visible;
                CalendarDescription.Text = Share.SharedStrings["Today_Description"];
            }

            if (Share.SharedStrings.ContainsKey("Today_Location") && !string.IsNullOrEmpty(Share.SharedStrings["Today_Location"]))
            {
                CalendarLocation.Visibility = Visibility.Visible;
                CalendarLocation.Text = Share.SharedStrings["Today_Location"];
            }

            if (Share.SharedStrings.ContainsKey("Today_Time"))
            {
                CalendarTime.Visibility = Visibility.Visible;
                CalendarTime.Text = Share.SharedStrings["Today_Time"];
            }
        }

        void TimerTick(object sender, EventArgs e)
        {
            Time.Text = DateTime.Now.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);
        }

        private double mouseY;

        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);
            mouseY = e.GetPosition((IInputElement)this.Parent).Y;
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            mouseY = 0;
            if (Translate.Y == 0)
            {
                var s = (Storyboard)Resources["JumpAnim"];
                s.Begin();
                return;
            }
            if (Translate.Y > -120)
            {
                var s = (Storyboard)Resources["MoveBackAnim"];
                s.Begin();
                return;
            }
            Unlock();
        }

        private void UserControlMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.Captured != this || e.LeftButton == MouseButtonState.Released)
                return;
            var y = e.GetPosition((IInputElement)this.Parent).Y;
            if (y >= mouseY)
                return;
            Translate.Y = y - mouseY;
        }

        private void UnlockAnimCompleted(object sender, EventArgs e)
        {
            if (Unlocked != null)
                Unlocked(this, EventArgs.Empty);
        }

        private void UserControlUnloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void MoveBackAnimCompleted(object sender, EventArgs e)
        {
            Translate.Y = 0;
        }

        public void Unlock()
        {
            var s = (Storyboard)Resources["UnlockAnim"];
            ((DoubleAnimation)s.Children[0]).To = -SystemParameters.PrimaryScreenHeight;
            s.Begin();
            timer.Stop();
        }

        private void JumpAnimCompleted(object sender, EventArgs e)
        {
            Translate.Y = 0;
        }

        private void NextClick(object sender, RoutedEventArgs e)
        {
            Share.SendMessage("Mosaic.Widgets", "Music:Next");
        }

        private void PlayPauseClick(object sender, RoutedEventArgs e)
        {
            Share.SendMessage("Mosaic.Widgets", "Music:PlayPause");
            PlayPause.IsChecked = !PlayPause.IsChecked;
        }

        private void PrevClick(object sender, RoutedEventArgs e)
        {
            Share.SendMessage("Mosaic.Widgets", "Music:Prev");
        }

        public void ChangeMedia()
        {
            if (Share.SharedStrings.ContainsKey("Music_CurrentMediaTitle"))
            {
                SongTitle.Text = Share.SharedStrings["Music_CurrentMediaTitle"];
            }

            if (Share.SharedStrings.ContainsKey("Music_CurrentMediaArtist"))
            {
                SongArtist.Text = Share.SharedStrings["Music_CurrentMediaArtist"];
            }

            if (Share.SharedStrings.ContainsKey("Music_CurrentMediaArt") && !string.IsNullOrEmpty(Share.SharedStrings["Music_CurrentMediaArt"]))
            {
                AlbumArt.Source = new BitmapImage(new Uri(Share.SharedStrings["Music_CurrentMediaArt"], UriKind.Absolute));
            }
        }
    }
}
