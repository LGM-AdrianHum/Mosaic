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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mosaic.Base;
using Social.Base;

namespace People
{
    /// <summary>
    /// Interaction logic for WallItem.xaml
    /// </summary>
    public partial class WallItem : UserControl
    {
        private WallEntry wallEntry;
        private WebClient webClient;

        public WallEntry WallEntry
        {
            get { return wallEntry; }
            set
            {
                wallEntry = value;
                Username.Text = wallEntry.FromName;
                if (string.IsNullOrEmpty(wallEntry.Message))
                    Message.Visibility = System.Windows.Visibility.Collapsed;
                else
                    Message.Text = wallEntry.Message;

                if (wallEntry.CommentsCount > 0)
                {
                    CommentsCount.Text = wallEntry.CommentsCount.ToString();
                    CommentsCountGrid.Visibility = System.Windows.Visibility.Visible;
                }

                if (File.Exists(E.Root + "\\Cache\\" + wallEntry.FromId + "_s.png"))
                {
                    try
                    {
                        var bi = new BitmapImage();
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.BeginInit();
                        bi.UriSource = new Uri(E.Root + "\\Cache\\" + wallEntry.FromId + "_s.png");
                        bi.EndInit();
                        Avatar.Source = bi; //new BitmapImage(new Uri(E.Root + "\\Cache\\" + wallEntry.FromId + "_s.png"));
                    }
                    catch { }
                }

                else if (!wallEntry.IsPage)
                {
                    webClient = new WebClient();
                    webClient.DownloadFileCompleted += WebClientDownloadFileCompleted;
                    webClient.DownloadFileAsync(new Uri(wallEntry.UserPic), E.Root + "\\Cache\\" + wallEntry.FromId + "_s.png");
                }
                else
                {
                    Avatar.Source = new BitmapImage(new Uri(wallEntry.UserPic));
                }
                //Avatar.Source = new BitmapImage(new Uri(wallEntry.UserPic));
                if (!string.IsNullOrEmpty(wallEntry.Application))
                    SentFrom.Text = wallEntry.Application;
                else
                    SentFrom.Text = "Facebook";
                SentFrom.Text += " " + wallEntry.CreatedTime.ToShortTimeString();

                if (!string.IsNullOrEmpty(wallEntry.Name))
                {
                    RepostPanel.Visibility = System.Windows.Visibility.Visible;
                    RepostTitle.Text = wallEntry.Name;
                    RepostText.Text = wallEntry.Description;
                    if (!string.IsNullOrEmpty(wallEntry.Picture))
                        RepostImage.Source = new BitmapImage(new Uri(wallEntry.Picture));
                }

                if (wallEntry.Comments == null)
                    CommentsPanel.Cursor = Cursors.Arrow;
            }
        }

        void WebClientDownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            webClient.DownloadFileCompleted -= WebClientDownloadFileCompleted;
            webClient.Dispose();
            var bi = new BitmapImage();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.BeginInit();
            bi.UriSource = new Uri(E.Root + "\\Cache\\" + wallEntry.FromId + "_s.png");
            bi.EndInit();
            Avatar.Source = bi;
            //Avatar.Source = new BitmapImage(new Uri(E.Root + "\\Cache\\" + wallEntry.FromId + "_s.png"));
        }

        public WallItem()
        {
            InitializeComponent();
        }
        private int order = 0;
        public int Order
        {
            get { return order; }
            set
            {
                order = value;
                TranslateAnim.BeginTime = TimeSpan.FromMilliseconds(150 + 80 * value);
                OpacityAnim.BeginTime = TimeSpan.FromMilliseconds(150 + 80 * value);
            }
        }

        private void RepostTitleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WinAPI.ShellExecute(IntPtr.Zero, "open", wallEntry.Link, null, null, 3);
        }

        private void UsernameMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WinAPI.ShellExecute(IntPtr.Zero, "open", wallEntry.EntryUrl, null, null, 3);
        }

        private void CommentsCountGridMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (wallEntry.Comments == null)
                return;

            if (CommentsPanel.Visibility == System.Windows.Visibility.Collapsed)
            {
                foreach (var comment in wallEntry.Comments)
                {
                    var item = new CommentItem();
                    item.WallComment = comment;
                    CommentsPanel.Children.Add(item);
                }

                CommentsPanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                CommentsPanel.Visibility = System.Windows.Visibility.Collapsed;
                CommentsPanel.Children.Clear();
            }
        }
    }
}
