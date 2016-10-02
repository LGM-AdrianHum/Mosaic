using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using System.Xml.Linq;
using Mosaic.Base;
using System.Collections;

namespace Today
{
    /// <summary>
    /// Interaction logic for HotmailWidget.xaml
    /// </summary>
    public partial class TodayWidget : UserControl
    {
        private Options optionsWindow;
        private WebClient webClient;
        private DispatcherTimer timer;
        private DispatcherTimer tileAnimTimer;

        public TodayWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            Share.SharedStrings.Add("Today_Title", null);
            Share.SharedStrings.Add("Today_Description", null);
            Share.SharedStrings.Add("Today_Location", null);
            Share.SharedStrings.Add("Today_Time", null);

            Day.Text = DateTime.Now.Day.ToString();
            Month.Text = DateTime.Now.ToString("MMMM");

            tileAnimTimer = new DispatcherTimer();
            tileAnimTimer.Interval = TimeSpan.FromSeconds(6);
            tileAnimTimer.Tick += TileAnimTimerTick;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(Widget.Settings.RefreshInterval);
            timer.Tick += TimerTick;

            webClient = new WebClient();
            if (string.IsNullOrEmpty(Widget.Settings.Username) || string.IsNullOrEmpty(Widget.Settings.Password))
            {
                Tip.Visibility = System.Windows.Visibility.Visible;
                return;
            }
            var cred = new NetworkCredential();
            cred.UserName = Widget.Settings.Username;
            cred.Password = Widget.Settings.Password;
            webClient.Credentials = cred;
            webClient.Encoding = Encoding.UTF8;

            GetFeed();

            timer.Start();
        }



        void TileAnimTimerTick(object sender, EventArgs e)
        {
            var s = (Storyboard)Resources["TileAnim"];
            s.Begin();
        }

        void TimerTick(object sender, EventArgs e)
        {
            Day.Text = DateTime.Now.Day.ToString();
            Month.Text = DateTime.Now.ToString("MMMM");

            GetFeed();
        }

        private void GetFeed()
        {
            // Connexion

            ThreadStart threadHotmail = () =>
                                            {
                                                var googleCalendar = new Calendar();

                                                CalendarData data = googleCalendar.GetFeedOfToday(Widget.Settings.FeedURL, Widget.Settings.Username, Widget.Settings.Password);
                                                this.Dispatcher.Invoke((Action)delegate
                                                {

                                                    if (data != null)
                                                    {
                                                        Title.Text = data.Title;

                                                        if (!string.IsNullOrEmpty(data.Location))
                                                        {
                                                            Location.Visibility = Visibility.Visible;
                                                            Location.Text = data.Location;
                                                        }
                                                        else
                                                            Location.Visibility = Visibility.Collapsed;

                                                        if (!string.IsNullOrEmpty(data.Description))
                                                        {
                                                            Description.Visibility = Visibility.Visible;
                                                            Description.Text = data.Description;
                                                        }
                                                        else
                                                            Description.Visibility = Visibility.Collapsed;

                                                        string beginTime = data.BeginTime.ToShortTimeString(); //by default show only the time
                                                        if (!IsToday(data.BeginTime))
                                                            beginTime = data.BeginTime.ToShortDateString() + " " + beginTime; //but if it's not today show date string too
                                                        string endTime = data.EndTime.ToShortTimeString();
                                                        if (!IsToday(data.EndTime))
                                                            endTime = data.EndTime.ToShortDateString() + " " + endTime;
                                                        Time.Text = beginTime + " - " + endTime;

                                                        Share.SharedStrings["Today_Title"] = Title.Text;
                                                        Share.SharedStrings["Today_Description"] = Description.Text;
                                                        Share.SharedStrings["Today_Location"] = Location.Text;
                                                        Share.SharedStrings["Today_Time"] = Time.Text;
                                                    }

                                                    // Récupération liste évènements à faire dans la journée (les 3 premiers)
                                                    //googleCalendar.getFeedOnDay();

                                                });
                                            };

            var thread = new Thread(threadHotmail);
            //thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private bool IsToday(DateTime date)
        {
            return (date.Year == DateTime.Now.Year && date.Month == DateTime.Now.Month && date.Day == DateTime.Now.Day);
        }

        private static readonly Action EmptyDelegate = delegate() { };

        public static void Refresh(UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        public void Unload()
        {
            timer.Tick -= TimerTick;
            timer.Stop();
            tileAnimTimer.Tick -= TileAnimTimerTick;
            tileAnimTimer.Stop();

            Share.SharedStrings.Remove("Today_Title");
            Share.SharedStrings.Remove("Today_Description");
            Share.SharedStrings.Remove("Today_Location");
            Share.SharedStrings.Remove("Today_Time");
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
            if (string.IsNullOrEmpty(Widget.Settings.Username) || string.IsNullOrEmpty(Widget.Settings.Password))
            {
                Tip.Visibility = System.Windows.Visibility.Visible;
                Title.Text = string.Empty;
                Description.Text = string.Empty;
                Location.Text = string.Empty;
                Time.Text = string.Empty;
                return;
            }
            else
                Tip.Visibility = System.Windows.Visibility.Collapsed;
            var cred = new NetworkCredential();
            cred.UserName = Widget.Settings.Username;
            cred.Password = Widget.Settings.Password;
            webClient.Credentials = cred;
            webClient.Encoding = Encoding.UTF8;
            GetFeed();
        }

        private void RefreshItemClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Widget.Settings.Username) || string.IsNullOrEmpty(Widget.Settings.Password))
                return;
            GetFeed();
        }



        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WinAPI.ShellExecute(IntPtr.Zero, "open", "https://www.google.com/calendar/", null, null, 1);
            /*
            if (Str2Int(UnreadCount.Text)>0)
            {
                WinAPI.ShellExecute(IntPtr.Zero, "open", Widget.Settings.MailHttp, null, null, 1);
                tileAnimTimer.Stop();
                // Reset unread messages number and refresh last saved total one
                Widget.Settings.LastMsgCount = Str2Int(MailCount.Text);
                UnreadCount.Text = "0";
            }
             */
        }
    }
}
