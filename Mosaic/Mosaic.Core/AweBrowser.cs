using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AwesomiumSharp;
using Mosaic.Base;
using UserControl = System.Windows.Controls.UserControl;

namespace Mosaic.Core
{
    public class AweBrowser : UserControl
    {
        private Image controlContent;
        private WebView webView;
        private DispatcherTimer updateTimer;
        private WriteableBitmap src;

        public event EventHandler Closed;

        public AweBrowser()
        {
            if (!WidgetManager.WebCoreInitialized)
            {
                WebCore.Initialize(new WebCore.Config());
                WidgetManager.WebCoreInitialized = true;
            }

            this.Loaded += AweBrowserLoaded;

            controlContent = new Image();
            controlContent.Source = src;
            controlContent.Width = Width;
            controlContent.Height = Height;
            this.Content = controlContent;
            this.MouseLeftButtonDown += AweBrowserMouseLeftButtonDown;
            this.MouseLeftButtonUp += AweBrowserMouseLeftButtonUp;
            //this.MouseMove += AweBrowserMouseMove;
        }

 
        void AweBrowserMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            /*var pos = e.GetPosition(this);
            webView.InjectMouseMove((int)pos.X, (int)pos.Y);*/
        }

        void AweBrowserMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            webView.InjectMouseUp(MouseButton.Left);
        }

        void AweBrowserMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Focus();
            controlContent.Focus();
            webView.Focus();
            var pos = e.GetPosition(this);
            webView.InjectMouseMove((int)pos.X, (int)pos.Y);
            webView.Focus();
            webView.InjectMouseDown(MouseButton.Left);
        }

        void AweBrowserLoaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        void UpdateTimerTick(object sender, EventArgs e)
        {
            WebCore.Update();

            if (webView.IsDirty())
            {
                Render();
            }
        }

        private void Render()
        {
            var rBuffer = webView.Render();
            rBuffer.CopyToBitmap(src);
            controlContent.Source = src;
        }

        public void NavigateToFile(string file)
        {
            src = new WriteableBitmap((int)Width, (int)Height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.WebPaletteTransparent);
            var html = File.ReadAllText(file);
            webView = WebCore.CreateWebview((int)Width, (int)Height);
            webView.LoadHTML(html);
            webView.OnBeginNavigation += webView_OnBeginNavigation;

            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromMilliseconds(15);
            updateTimer.Tick += UpdateTimerTick;
            updateTimer.Start();
        }


        void webView_OnBeginNavigation(object sender, WebView.BeginNavigationEventArgs e)
        {
            if (e.url == "local://base_request.html/close" && Closed != null)
                Closed(this, EventArgs.Empty);

        }

        public void ProcessKeyboardInput(System.Windows.Input.KeyEventArgs e)
        {
            var k = new WebKeyboardEvent();
            k.nativeKeyCode = (int)e.Key;
            k.type = WebKeyType.Char;
            k.virtualKeyCode = (int) e.Key;
            webView.InjectKeyboardEvent(k);
        }

        public void ProcessKeyboardInput(int msg, int wParam, int lParam)
        {
            webView.InjectKeyboardEventWin(msg, wParam, lParam);
        }

        public void Close()
        {
            this.Loaded -= AweBrowserLoaded;
            this.MouseLeftButtonDown -= AweBrowserMouseLeftButtonDown;
            this.MouseLeftButtonUp -= AweBrowserMouseLeftButtonUp;
            //this.MouseMove -= AweBrowserMouseMove;
            webView.OnBeginNavigation -= webView_OnBeginNavigation;

            updateTimer.Stop();
            webView.Dispose();
        }
    }
}
