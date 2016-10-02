using System;
using System.Collections.Generic;
using System.Linq;
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
using Social.Base;

namespace People
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
