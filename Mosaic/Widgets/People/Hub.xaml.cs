using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Social.Base;

namespace People
{
    /// <summary>
    /// Interaction logic for Hub.xaml
    /// </summary>
    public partial class Hub : UserControl
    {
        public event EventHandler Close;
        private SocialProvider socialProvider;

        public Hub()
        {
            InitializeComponent();
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Close != null)
                Close(this, null);
        }

        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            socialProvider = new SocialProvider();
            socialProvider.SignedIn += SocialProviderSignedIn;
            ThreadStart threadStarter = () => socialProvider.SignIn();
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        void SocialProviderSignedIn(object sender, EventArgs e)
        {
            socialProvider.SignedIn -= SocialProviderSignedIn;

            ThreadStart threadStarter = () =>
            {
                var entries = socialProvider.GetFriendStream();
                this.Dispatcher.Invoke((Action)delegate
                {
                    foreach (var entry in entries)
                    {
                        var item = new WallItem();
                        item.WallEntry = entry;
                        item.Order = FeedPanel.Children.Count;
                        FeedPanel.Children.Add(item);
                    }

                    ProgressBar.IsIndeterminate = false;
                    ProgressBar.Visibility = Visibility.Collapsed;
                });
            };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

        }
    }
}
