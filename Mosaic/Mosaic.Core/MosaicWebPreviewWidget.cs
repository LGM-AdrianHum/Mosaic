using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Mosaic.Base;
using ContextMenu = System.Windows.Controls.ContextMenu;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Image = System.Windows.Controls.Image;
using MenuItem = System.Windows.Controls.MenuItem;
using WebBrowser = System.Windows.Forms.WebBrowser;

namespace Mosaic.Core
{
    public class MosaicWebPreviewWidget : MosaicWidget
    {
        private Image previewControl;
        private System.Windows.Forms.WebBrowser browser;
        private string url;
        private string file;

        public override string Name
        {
            get { return string.Empty; }
        }

        public override System.Windows.FrameworkElement WidgetControl
        {
            get { return previewControl; }
        }

        public override Uri IconPath
        {
            get { return null; }
        }

        public override int ColumnSpan
        {
            get { return 1; }
        }

        public override void Load(string path)
        {
            url = path;
            previewControl = new Image();
            previewControl.Stretch = Stretch.UniformToFill;
            previewControl.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(PreviewControlMouseLeftButtonUp);
            RenderOptions.SetBitmapScalingMode(previewControl, BitmapScalingMode.HighQuality);
            previewControl.HorizontalAlignment = HorizontalAlignment.Left;

            file = ConvertUrlToFileName(path) + ".png";
            if (File.Exists(E.Root + "\\Thumbnails\\" + file))
            {
                var bi = new BitmapImage();

                bi.BeginInit();

                bi.CacheOption = BitmapCacheOption.OnLoad;

                bi.UriSource = new Uri(E.Root + "\\Thumbnails\\" + file);

                bi.EndInit();
                previewControl.Source = bi;
                //previewControl.Source = new BitmapImage(new Uri(E.Root + "\\Thumbnails\\" + file));
            }
            else
            {
                browser = new WebBrowser();
                browser.ScrollBarsEnabled = false;
                browser.ScriptErrorsSuppressed = true;
                //browser.Navigated += BrowserNavigated;
                browser.DocumentCompleted += BrowserDocumentCompleted;
                browser.Width = 1024;
                browser.Height = 768;
                browser.Navigate(path);
            }
        }

        public override void Refresh()
        {
            browser = new WebBrowser();
            browser.ScrollBarsEnabled = false;
            browser.ScriptErrorsSuppressed = true;
            browser.DocumentCompleted += BrowserDocumentCompleted;
            browser.Width = 1024;
            browser.Height = 768;
            browser.Navigate(url);
        }

        void PreviewControlMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WinAPI.ShellExecute(IntPtr.Zero, "open", url, null, null, 1);
        }

        void BrowserDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (browser.ReadyState != WebBrowserReadyState.Complete)
                return;
            browser.DocumentCompleted -= BrowserDocumentCompleted;
            var bitmap = new Bitmap(browser.Width, browser.Height);
            browser.DrawToBitmap(bitmap, new System.Drawing.Rectangle(0, 0, browser.Width, browser.Height));
            try
            {
                previewControl.Source = null;
                if (File.Exists(E.Root + "\\Thumbnails\\" + file))
                    File.Delete(E.Root + "\\Thumbnails\\" + file);
                bitmap.Save(E.Root + "\\Thumbnails\\" + file, ImageFormat.Png);
            }
            catch { }
            if (File.Exists(E.Root + "\\Thumbnails\\" + file))
            {
                var bi = new BitmapImage();

                bi.BeginInit();

                bi.CacheOption = BitmapCacheOption.OnLoad;

                bi.UriSource = new Uri(E.Root + "\\Thumbnails\\" + file);

                bi.EndInit();
                previewControl.Source = bi;
                /*previewControl.Source = new BitmapImage(new Uri(E.Root + "\\Thumbnails\\" + file));
                    //ToBitmapSource(bitmap);*/
            }
            bitmap.Dispose();
            browser.Dispose();
        }

        public override void Unload()
        {
            if (browser != null && !browser.IsDisposed)
                browser.Dispose();

            previewControl.MouseLeftButtonUp -= PreviewControlMouseLeftButtonUp;
        }

        private static string ConvertUrlToFileName(string url)
        {
            return Path.GetFileName(Uri.UnescapeDataString(url).Replace("/", "\\").Replace("?", "-").Replace(":", "-"));
        }
    }
}
