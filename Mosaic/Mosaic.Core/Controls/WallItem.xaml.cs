using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Mosaic.Base;
using Social.Base;

namespace Mosaic.Core.Controls
{
    /// <summary>
    /// Interaction logic for WallItem.xaml
    /// </summary>
    public partial class WallItem : UserControl
    {
        private WallEntry wallEntry;
        public WallItem()
        {
            InitializeComponent();
        }

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

                Avatar.Source = new BitmapImage(new Uri(wallEntry.UserPic));
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
            }
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
