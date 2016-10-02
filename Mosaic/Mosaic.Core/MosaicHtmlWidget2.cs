using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using Mosaic.Base;

namespace Mosaic.Core
{

    public class MosaicHtmlWidget2 : MosaicWidget
    {
        private string rootPath;
        private string name;
        private int colSpan;
        private string contentPath;
        private string optionsContentPath;
        private string hubContentPath;
        private string iconPath;
        private ContextMenu contextMenu;
        private MenuItem optionsItem;
        private Window optionsWindow;
        private HubWindow hub;

        private AweBrowser contentBrowser;
        private AweBrowser optionsBrowser;
        private AweBrowser hubBrowser;

        public MosaicHtmlWidget2(string root)
        {
            rootPath = root;
            if (!File.Exists(rootPath + "\\Widget.Description.xml"))
                throw new FileNotFoundException("Description file " + rootPath + "\\Widget.Description.xml not found.");
            var xml = XElement.Load(rootPath + "\\Widget.Description.xml");
            name = xml.Element("Name").Value;
            colSpan = int.Parse(xml.Element("ColSpan").Value);
            contentPath = xml.Element("Content").Value;
            if (!File.Exists(rootPath + "\\" + contentPath))
                throw new FileNotFoundException("Content file " + rootPath + "\\" + contentPath + " not found.");
            if (xml.Element("Icon") != null)
                iconPath = rootPath + "\\" + xml.Element("Icon").Value;

            if (xml.Element("Options") != null)
            {
                optionsContentPath = xml.Element("Options").Value;
                if (!File.Exists(rootPath + "\\" + optionsContentPath))
                    throw new FileNotFoundException("Options file " + rootPath + "\\" + optionsContentPath + " not found.");
            }

            if (xml.Element("Hub") != null)
            {
                hubContentPath = xml.Element("Hub").Value;
                if (!File.Exists(rootPath + "\\" + hubContentPath))
                    throw new FileNotFoundException("Hub content file " + rootPath + "\\" + hubContentPath + " not found.");
            }

        }

        public override string Name
        {
            get { return name; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return contentBrowser; }
        }

        public override Uri IconPath
        {
            get
            {
                if (string.IsNullOrEmpty(iconPath))
                    return null;
                return new Uri(iconPath);

            }
        }

        public override int ColumnSpan
        {
            get { return colSpan; }
        }

        public override void Load()
        {
            contentBrowser = new AweBrowser();
            contentBrowser.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0f92d6"));
            contentBrowser.Width = E.MinTileWidth * colSpan;
            contentBrowser.Height = E.MinTileHeight;
            contentBrowser.NavigateToFile(rootPath + "\\" + contentPath);
            contentBrowser.MouseLeftButtonDown += ContentBrowserMouseLeftButtonDown;
            contentBrowser.MouseLeftButtonUp += ContentBrowserMouseLeftButtonUp;

            if (!string.IsNullOrEmpty(optionsContentPath))
            {
                contextMenu = new ContextMenu();
                optionsItem = new MenuItem();
                optionsItem.Header = "Options";
                optionsItem.Click += OptionsItemClick;
                contextMenu.Items.Insert(0, optionsItem);
                contentBrowser.ContextMenu = contextMenu;
            }

            if (!string.IsNullOrEmpty(hubContentPath))
            {
                hub = new HubWindow();
            }
        }

        private Point mouseDown;
        void ContentBrowserMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseDown = e.GetPosition(contentBrowser);
        }

        void ContentBrowserMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(contentBrowser);
            if (mouseDown.X - 5 > pos.X || mouseDown.X + 5 < pos.X)
                return;
            if (mouseDown.Y - 5 > pos.Y || mouseDown.Y + 5 < pos.Y)
                return;

            if (string.IsNullOrEmpty(hubContentPath))
                return;

            if (hub != null && hub.IsVisible)
            {
                hub.Activate();
                return;
            }

            hub = new HubWindow();
            hub.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            hub.Width = SystemParameters.PrimaryScreenWidth;
            hub.Height = SystemParameters.PrimaryScreenHeight;
            hub.Left = 0;
            hub.Top = 0;
            hubBrowser = new AweBrowser();
            hubBrowser.Closed += HubBrowserClosed;
            hubBrowser.Width = hub.Width;
            hubBrowser.Height = hub.Height;
            hub.Content = hubBrowser;
            hub.SourceInitialized += HubSourceInitialized;


            if (E.Language == "he-IL" || E.Language == "ar-SA")
            {
                hub.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                hub.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            hubBrowser.NavigateToFile(rootPath + "\\" + hubContentPath);
            hub.ShowDialog();
        }

        void HubSourceInitialized(object sender, EventArgs e)
        {
            hub.SourceInitialized -= HubSourceInitialized;
            var source = (HwndSource)PresentationSource.FromVisual(hub);
            source.AddHook(HandleMessages);
        }

        IntPtr HandleMessages(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            int message = (msg & 65535);

            if ((message == WinAPI.WM_KEYDOWN || message == WinAPI.WM_KEYUP || message == WinAPI.WM_CHAR))
            {
                hubBrowser.ProcessKeyboardInput(msg, (int)wParam, (int)lParam);
                handled = true;
            }

            return IntPtr.Zero;
        }

        void HubBrowserClosed(object sender, EventArgs e)
        {
            hubBrowser.Closed -= HubBrowserClosed;
            hub.Close();
        }

        void OptionsItemClick(object sender, RoutedEventArgs e)
        {
            if (optionsWindow != null && optionsWindow.IsVisible)
            {
                optionsWindow.Activate();
                return;
            }

            optionsWindow = new Window();
            optionsWindow.Width = 380;
            optionsWindow.Height = 410;
            optionsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            optionsWindow.Closed += new EventHandler(OptionsWindowClosed);
            optionsBrowser = new AweBrowser();
            optionsBrowser.Width = optionsWindow.Width;
            optionsBrowser.Height = optionsWindow.Height;
            optionsWindow.Content = optionsBrowser;

            if (E.Language == "he-IL" || E.Language == "ar-SA")
            {
                optionsWindow.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                optionsWindow.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            optionsBrowser.NavigateToFile(rootPath + "\\" + optionsContentPath);
            optionsBrowser.Closed += OptionsBrowserClosed;
            optionsWindow.ShowDialog();
        }

        void OptionsWindowClosed(object sender, EventArgs e)
        {
            optionsWindow.Closed -= OptionsWindowClosed;
            optionsBrowser.Close();
        }

        void OptionsBrowserClosed(object sender, EventArgs e)
        {
            optionsBrowser.Closed -= OptionsBrowserClosed;
            optionsWindow.Close();
        }


        public override void Unload()
        {
            contentBrowser.MouseLeftButtonDown -= ContentBrowserMouseLeftButtonDown;
            contentBrowser.MouseLeftButtonUp -= ContentBrowserMouseLeftButtonUp;

            contentBrowser.Close();
        }
    }
}
