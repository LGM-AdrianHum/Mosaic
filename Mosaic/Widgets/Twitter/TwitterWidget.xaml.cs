using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using Mosaic.Base;
using TweetSharp;

namespace Twitter
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TwitterWidget : UserControl
    {
        private Options optionsWindow;
        public static TwitterService Service;
        private string lastTweetUrl;
        private DispatcherTimer tileAnimTimer;
        private DispatcherTimer timer;

        public TwitterWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            tileAnimTimer = new DispatcherTimer();
            tileAnimTimer.Interval = TimeSpan.FromSeconds(6);
            tileAnimTimer.Tick += TileAnimTimerTick;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(Widget.Settings.RefreshInterval);
            timer.Tick += TimerTick;

            Service = new TwitterService("4i6I1Jo68Kqje8yD69r8Q", "ocDQ8CHRRCz78TcdSFnjMqXvz4wPDBYsCJ5hFZ9phDY");
            if (string.IsNullOrEmpty(Widget.Settings.AccessToken) || string.IsNullOrEmpty(Widget.Settings.AccessTokenSecret))
            {
                return;
            }

            timer.Start();

            Tip.Text = Properties.Resources.OptionsNoTweets;
            Service.AuthenticateWith(Widget.Settings.AccessToken, Widget.Settings.AccessTokenSecret);
            GetLatestTweet();
        }

        void TileAnimTimerTick(object sender, EventArgs e)
        {
            var s = (Storyboard)Resources["TileAnim"];
            s.Begin();
        }

        private void RefreshItemClick(object sender, RoutedEventArgs e)
        {
            GetLatestTweet();
        }

        void TimerTick(object sender, EventArgs e)
        {
            GetLatestTweet();
        }

        private void OptionsItemClick(object sender, RoutedEventArgs e)
        {
            ShowOptions();
        }

        private void ShowOptions()
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
            if (string.IsNullOrEmpty(Widget.Settings.AccessToken) || string.IsNullOrEmpty(Widget.Settings.AccessTokenSecret))
            {
                Tip.Visibility = System.Windows.Visibility.Visible;
                return;
            }

            Tip.Visibility = System.Windows.Visibility.Collapsed;
            SignIn();
            timer.Stop();
            timer.Start();
            //GetLatestTweet();
        }

        private void SignIn()
        {
            ThreadStart threadStarter = () =>
            {
                Service.AuthenticateWith(Widget.Settings.AccessToken, Widget.Settings.AccessTokenSecret);
                GetLatestTweet();
            };
            var thread = new Thread(threadStarter);
            thread.Start();
        }

        private void GetLatestTweet()
        {
            ThreadStart threadStarter = () =>
                                            {
                                                TwitterRateLimitStatus rates;
                                                try
                                                {
                                                    rates = Service.GetRateLimitStatus();
                                                }
                                                catch
                                                {
                                                    return;
                                                }
                                                if (rates == null || rates.RemainingHits == 0)
                                                    return;
                                                IEnumerable<TwitterStatus> tweets = null;
                                                if (Widget.Settings.LastTweetId > 0)
                                                    tweets = Service.ListTweetsOnFriendsTimelineSince(Widget.Settings.LastTweetId);
                                                else
                                                    tweets = Service.ListTweetsOnFriendsTimeline();
                                                TwitterStatus lastTweet;
                                                if (tweets != null && tweets.Count() > 0)
                                                {
                                                    this.Dispatcher.Invoke((Action)delegate
                                                                                           {
                                                                                               Tip.Visibility = Visibility.Collapsed;
                                                                                           });
                                                    UnreadCount.Dispatcher.Invoke((Action)delegate
                                                                                               {
                                                                                                   UnreadCount.Text = tweets.Count().ToString();
                                                                                               });
                                                    lastTweet = tweets.First();
                                                }
                                                else
                                                {
                                                    this.Dispatcher.Invoke((Action)delegate
                                                                                   {
                                                                                       Tip.Text = Properties.Resources.OptionsNoTweets;
                                                                                       Tip.Visibility = Visibility.Visible;
                                                                                   });
                                                    return;
                                                }
                                                //lastTweet = Service.ListTweetsOnFriendsTimeline(1).First();
                                                this.Dispatcher.Invoke((Action)delegate
                                                                                   {
                                                                                       var userpic = lastTweet.User.ProfileImageUrl.Replace("_normal", "");

                                                                                       UserPic.Source = new BitmapImage(new Uri(userpic));
                                                                                       Username.Text = lastTweet.User.ScreenName;
                                                                                       Tweet.Text = lastTweet.Text;
                                                                                       lastTweetUrl = string.Format("http://twitter.com/#!/{0}/status/{1}", lastTweet.User.ScreenName, lastTweet.Id);
                                                                                       if (Widget.Settings.LastTweetId.ToString() != lastTweet.Id.ToString())
                                                                                       {
                                                                                           Widget.Settings.LastTweetId = long.Parse(lastTweet.Id.ToString());
                                                                                           tileAnimTimer.Start();
                                                                                       }
                                                                                   });
                                            };
            var thread = new Thread(threadStarter);
            thread.Start();
        }

        public void Unload()
        {
            timer.Tick -= TimerTick;
            timer.Stop();
            tileAnimTimer.Tick -= TileAnimTimerTick;
            tileAnimTimer.Stop();
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(Widget.Settings.AccessToken) || string.IsNullOrEmpty(Widget.Settings.AccessTokenSecret))
            {
                ShowOptions();
                return;
            }

            if (string.IsNullOrEmpty(lastTweetUrl))
            {
                Process.Start("http://twitter.com");
                return;
            }
            Process.Start(lastTweetUrl);
            tileAnimTimer.Stop();
            UnreadCount.Text = "0";
        }
    }
}
