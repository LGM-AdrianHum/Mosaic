using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Windows.Threading;
using Mosaic.Base;
using Path = System.Windows.Shapes.Path;

namespace Me
{
    /// <summary>
    /// Interaction logic for MeWidget.xaml
    /// </summary>
    public partial class MeWidget : UserControl
    {
        private DispatcherTimer tileAnimTimer;

        public MeWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            //if (string.IsNullOrEmpty(Widget.Settings.PicturePath))
            //{
            //    var userpic = System.IO.Path.GetTempPath() + "\\" + Environment.UserName + ".bmp";
            //    if (File.Exists(userpic))
            //    {
            //        UserPic.Source = new BitmapImage(new Uri(userpic));
            //    }
            //}
            //else
            //    UserPic.Source = new BitmapImage(new Uri(Widget.Settings.PicturePath));
            if (File.Exists(E.Root + "\\Cache\\user.png"))
            {
                LoadUserPicFromCache();
            }
            else if (File.Exists(System.IO.Path.GetTempPath() + "\\" + Environment.UserName + ".bmp"))
            {
                File.Copy(System.IO.Path.GetTempPath() + "\\" + Environment.UserName + ".bmp", E.Root + "\\Cache\\user.png", true);
                LoadUserPicFromCache();
            }

            UserPic.Width = E.MinTileWidth;
            UserPic.Height = E.MinTileHeight;
            UserName.Text = Environment.UserName;

            tileAnimTimer = new DispatcherTimer();
            tileAnimTimer.Interval = TimeSpan.FromSeconds(14);
            tileAnimTimer.Tick += TileAnimTimerTick;
            if (E.AnimationEnabled)
                tileAnimTimer.Start();
        }

        private void LoadUserPicFromCache()
        {
            //if (UserPic.Source != null)
            //{
            //    ((BitmapImage)UserPic.Source).StreamSource.Close();
            //}
            
            //var bi = new BitmapImage();
            //bi.BeginInit();
            //bi.CacheOption = BitmapCacheOption.OnLoad;
            //bi.UriSource = new Uri(E.Root + "\\Cache\\user.png");
            //bi.EndInit();
            //UserPic.Source = bi;

            var ms = new MemoryStream();
            var stream = new FileStream(E.Root + "\\Cache\\user.png", FileMode.Open, FileAccess.Read);
            ms.SetLength(stream.Length);
            stream.Read(ms.GetBuffer(), 0, (int)stream.Length);

            ms.Flush();
            stream.Close();

            var src = new BitmapImage();
            src.BeginInit();
            src.StreamSource = ms;
            src.EndInit();
            UserPic.Source = src;
        }

        public void Unload()
        {
            tileAnimTimer.Tick -= TileAnimTimerTick;
            tileAnimTimer.Stop();
        }

        void TileAnimTimerTick(object sender, EventArgs e)
        {
            var s = (Storyboard)Resources["TileAnim"];
            s.Begin();
        }

        private void UserControlDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
        }

        private void UserControlDrop(object sender, DragEventArgs e)
        {
            var file = from x in ((string[])e.Data.GetData(DataFormats.FileDrop, true))
                       where x.EndsWith(".png") || x.EndsWith(".jpg")
                       select x;
            var path = file.First();
            File.Copy(path, E.Root + "\\Cache\\user.png", true);
            LoadUserPicFromCache();
        }
    }
}
