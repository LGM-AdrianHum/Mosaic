using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using Mosaic.Base;
using Mosaic.Controls;
using Social.Base;

namespace Mosaic.Windows
{
    /// <summary>
    /// Interaction logic for PeopleHub.xaml
    /// </summary>
    public partial class PeopleHub : Window
    {
        private SocialProvider socialProvider;

        public PeopleHub()
        {
            InitializeComponent();
        }

        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            socialProvider = new SocialProvider();
            socialProvider.SignedIn += SocialProviderSignedIn;
            ThreadStart threadStarter = delegate
                                            {
                                                socialProvider.SignIn();
                                            };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        void SocialProviderSignedIn(object sender, EventArgs e)
        {
            socialProvider.SignedIn -= SocialProviderSignedIn;
            ThreadStart threadStarter = delegate
            {
                var friends = socialProvider.GetFriends();
                this.Dispatcher.BeginInvoke((Action)delegate
                                                        {
                                                            char lastChar = ' ';
                                                            foreach (var friend in friends)
                                                            {
                                                                if (lastChar != friend.Name[0])
                                                                {
                                                                    lastChar = friend.Name[0];
                                                                    var catControl = new PeopleCategoryControl();
                                                                    catControl.Title = lastChar;
                                                                    PeoplePanel.Children.Add(catControl);
                                                                }
                                                                var item = new PeopleItem();
                                                                item.Friend = friend;
                                                                var loadedFriend = App.WidgetManager.Widgets.Find(x => x.Path == friend.Id);
                                                                if (loadedFriend != null)
                                                                {
                                                                    item.IsChecked = true;
                                                                    AddFavoriteFriend(friend.Id);
                                                                }
                                                                item.MouseLeftButtonUp += ItemMouseLeftButtonUp;
                                                                PeoplePanel.Children.Add(item);
                                                                ProgressBar.IsIndeterminate = false;
                                                                ProgressBar.Visibility = Visibility.Collapsed;
                                                            }
                                                        });
            };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            ThreadStart feedsThreadStarter = delegate
                                                 {
                                                     var wall = socialProvider.GetFriendStream();
                                                     if (wall.Count > 0)
                                                     {
                                                         WallEntry lastUpdate = null;
                                                         foreach (var wallEntry in wall)
                                                         {
                                                             if (string.IsNullOrEmpty(wallEntry.Message))
                                                                 continue;
                                                             lastUpdate = wallEntry;
                                                             break;
                                                         }
                                                         this.Dispatcher.BeginInvoke((Action)delegate
                                                                                                  {
                                                                                                      if (!File.Exists(E.Root + "\\Cache\\" + lastUpdate.FromId + ".png"))
                                                                                                          SetImage(LastUpdateImage, new Uri(string.Format("https://graph.facebook.com/{0}/picture?type=normal", lastUpdate.FromId)));
                                                                                                      else
                                                                                                          SetImage(LastUpdateImage, new Uri(string.Format(E.Root + "\\Cache\\{0}.png", lastUpdate.FromId)));
                                                                                                      LastUpdateText.Text = '"' + lastUpdate.Message + '"';
                                                                                                  });
                                                     }
                                                 };
            var feedsThread = new Thread(feedsThreadStarter);
            feedsThread.SetApartmentState(ApartmentState.STA);
            feedsThread.Start();
        }

        private void AddFavoriteFriend(string id)
        {
            FavoritesRootPanel.Visibility = Visibility.Visible;
            var favoriteFriend = new Image();
            favoriteFriend.Width = 100;
            favoriteFriend.Height = 100;
            favoriteFriend.Margin = new Thickness(5);
            favoriteFriend.Stretch = Stretch.UniformToFill;
            if (!File.Exists(E.Root + "\\Cache\\" + id + ".png"))
                SetImage(favoriteFriend, new Uri(string.Format("https://graph.facebook.com/{0}/picture?type=normal", id)));
            else
                SetImage(favoriteFriend, new Uri(string.Format(E.Root + "\\Cache\\{0}.png", id)));
            FavoritesPanel.Children.Add(favoriteFriend);
        }

        private void SetImage(Image image, Uri source)
        {
            var bi = new BitmapImage();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.BeginInit();
            bi.UriSource = source;
            bi.EndInit();
            image.Source = bi;
        }

        void ItemMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = (PeopleItem)sender;
            if (App.WidgetManager.IsWidgetLoaded(item.Friend.Name))
                return;
            AddFavoriteFriend(item.Friend.Id);
            var widget = App.WidgetManager.CreateFriendWidget(item.Friend.Id, item.Friend.Name);
            App.WidgetManager.LoadWidget(widget);
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
