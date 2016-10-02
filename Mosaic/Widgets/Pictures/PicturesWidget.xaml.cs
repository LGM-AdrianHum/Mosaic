using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Shell;
using Mosaic.Base;
using Path = System.IO.Path;

namespace Pictures
{
    /// <summary>
    /// Interaction logic for PicturesWidget.xaml
    /// </summary>
    public partial class PicturesWidget : UserControl
    {
        private string path = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Web\Wallpaper";
        private List<string> pictures = new List<string>();
        private Random random;
        private DispatcherTimer timer;

        public PicturesWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += new EventHandler(TimerTick);
            timer.Start();

            pictures = new List<string>();
            if (!ShellLibrary.IsPlatformSupported)
            {
                if (Directory.Exists(path))
                {
                    var pics = from x in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                               where x.ToLower().EndsWith("jpg") || x.ToLower().EndsWith("png")
                               select x;
                    foreach (var p in pics)
                    {
                        pictures.Add(p);
                    }
                }
            }
            else
            {
                var lib = ShellLibrary.Load(KnownFolders.PicturesLibrary, true);
                foreach (var l in lib)
                {

                    pictures.AddRange(from x in Directory.GetFiles(l.Path, "*.*", SearchOption.AllDirectories) where x.ToLower().EndsWith("jpg") || x.ToLower().EndsWith("png") select x);
                }
                lib.Dispose();
            }

            random = new Random(Environment.TickCount);
            if (pictures.Count > 0)
            {
                //Picture.Source = new BitmapImage(new Uri(pictures[random.Next(0, pictures.Count - 1)]));
                LoadPicture(pictures[random.Next(0, pictures.Count - 1)], Picture);
            }
        }

        private void LoadPicture(string path, Image image)
        {
            var bi = new BitmapImage();
            bi.BeginInit();

            bi.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);

            bi.DecodePixelWidth = (int)E.MinTileWidth * 2;
            bi.EndInit();

            image.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                image.Source = bi;
            });
        }

        void TimerTick(object sender, EventArgs e)
        {
            if (pictures.Count <= 0) return;
            LoadPicture(pictures[random.Next(0, pictures.Count - 1)], PictureBg);
            //PictureBg.Source = new BitmapImage(new Uri(pictures[random.Next(0, pictures.Count - 1)]));

            var s = (Storyboard)Resources["SwitchPictureAnim"];
            s.Begin();
        }

        private void SwitchAnimationCompleted(object sender, EventArgs e)
        {
            Picture.Source = PictureBg.Source;
            PictureBg.Source = null;
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var file = ((BitmapImage) Picture.Source).UriSource.OriginalString;
            var p = new ProcessStartInfo("explorer.exe");
            p.Arguments = "/select, " + file;
            p.WindowStyle = ProcessWindowStyle.Maximized;
            Process.Start(p);
        }
    }
}
