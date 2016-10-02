using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Social.Base;

namespace Mosaic.Core.Controls
{
    /// <summary>
    /// Interaction logic for CommentItem.xaml
    /// </summary>
    public partial class CommentItem : UserControl
    {
        private WallComment wallComment;

        public WallComment WallComment
        {
            get { return wallComment; }
            set
            {
                wallComment = value;
                Username.Text = wallComment.FromName;
                Message.Text = wallComment.Message;
                Avatar.Source = new BitmapImage(new Uri(string.Format("http://graph.facebook.com/{0}/picture?type=square", wallComment.FromId)));
                Date.Text = wallComment.CreatedTime.ToShortTimeString();
            }
        }

        public CommentItem()
        {
            InitializeComponent();
        }
    }
}
