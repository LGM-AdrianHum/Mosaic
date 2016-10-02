using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using Mosaic.Base;
using Social.Base;

namespace Mosaic.Controls
{
    /// <summary>
    /// Interaction logic for PeopleItem.xaml
    /// </summary>
    public partial class PeopleItem : UserControl
    {
        public bool MousePressed;
        private WebClient webClient;

        public PeopleItem()
        {
            InitializeComponent();
        }

        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (value)
                {
                    Title.Foreground = Brushes.Gray;
                    Image.Opacity = 0.4;
                }
                else
                {
                    Title.Foreground = Brushes.White;
                    Image.Opacity = 1;
                }
                isChecked = value;
            }
        }

        private Friend friend;
        public Friend Friend
        {
            get { return friend; }
            set
            {
                friend = value;
                Title.Text = value.Name;
                if (File.Exists(E.Root + "\\Cache\\" + friend.Id + "_s.png"))
                {
                    var info = new FileInfo(E.Root + "\\Cache\\" + friend.Id + "_s.png");
                    if (info.Length == 0)
                    {
                        DownloadUserpic(friend);
                        return;
                    }
                    try
                    {
                        Image.Source = new BitmapImage(new Uri(E.Root + "\\Cache\\" + friend.Id + "_s.png"));
                    }
                    catch (Exception ex)
                    {
                        App.Logger.Error("Can't set friend picture. " + ex);
                    }
                    return;
                }

                DownloadUserpic(value);
            }
        }

        private void DownloadUserpic(Friend friend)
        {
            webClient = new WebClient();
            webClient.DownloadFileCompleted += WebClientDownloadFileCompleted;
            webClient.DownloadFileAsync(new Uri(string.Format("https://graph.facebook.com/{0}/picture?type=square", friend.Id)), E.Root + "\\Cache\\" + friend.Id + "_s.png");
        }

        void WebClientDownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            webClient.DownloadFileCompleted -= new System.ComponentModel.AsyncCompletedEventHandler(WebClientDownloadFileCompleted);
            webClient.Dispose();
            try
            {
                Image.Source = new BitmapImage(new Uri(E.Root + "\\Cache\\" + friend.Id + "_s.png"));
            }
            catch (Exception ex)
            {
                App.Logger.Error("Can't set friend picture. " + ex);
            }
        }

        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var s = Resources["MouseDownAnim"] as Storyboard;
            s.Begin();
            MousePressed = true;
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var s = Resources["MouseUpAnim"] as Storyboard;
            s.Begin();
            MousePressed = false;
            if (!IsChecked)
                IsChecked = true;
        }

        private void UserControlMouseLeave(object sender, MouseEventArgs e)
        {
            if (!MousePressed)
                return;
            var s = Resources["MouseUpAnim"] as Storyboard;
            s.Begin();
            MousePressed = false;
        }
    }
}
