using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Mosaic.Base;
using Mosaic.Core.Controls;

namespace Mosaic.Core
{
    public class MosaicFriendWidget : MosaicWidget
    {
        private MosaicFriendWidgetControl control;
        private WebClient webClient;
        private string id;

        public override string Name
        {
            get { return null; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return control; }
        }

        public override Uri IconPath
        {
            get { return null; }
        }

        public override int ColumnSpan
        {
            get { return 1; }
        }

        public override void Load(string id, string name, int seed)
        {
            this.id = id;
            control = new MosaicFriendWidgetControl(seed);
            control.UserName.Text = name;
            control.MouseLeftButtonUp += ControlMouseLeftButtonUp;
            control.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0f92d6"));

            if (!File.Exists(E.Root + "\\Cache\\" + id + ".png"))
            {
                webClient = new WebClient();
                webClient.DownloadFileCompleted += WebClientDownloadFileCompleted;
                webClient.DownloadFileAsync(new Uri(string.Format("https://graph.facebook.com/{0}/picture?type=large", id)), E.Root + "\\Cache\\" + id + ".png");
            }
            else
            {
                var bi = new BitmapImage();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.BeginInit();
                bi.UriSource = new Uri(E.Root + "\\Cache\\" + id + ".png");
                bi.EndInit();
                control.UserPic.Source = bi;
            }
        }

        void ControlMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var hub = new MosaicFriendWidgetHub(id);
            hub.Show();
        }

        public override void Unload()
        {
            control.MouseLeftButtonUp -= ControlMouseLeftButtonUp;
        }

        void WebClientDownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            webClient.DownloadFileCompleted -= WebClientDownloadFileCompleted;
            if (e.Error != null)
                return;
            var bi = new BitmapImage();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.BeginInit();
            bi.UriSource = new Uri(E.Root + "\\Cache\\" + id + ".png");
            bi.EndInit();
            control.UserPic.Source = bi;
            //control.UserPic.Source = new BitmapImage(new Uri(E.Root + "\\Cache\\" + id + ".png"));
        }
    }
}
