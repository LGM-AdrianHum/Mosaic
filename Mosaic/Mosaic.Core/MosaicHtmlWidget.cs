using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using Mosaic.Base;
using mshtml;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Mosaic.Core
{
    public class MosaicHtmlWidget : MosaicWidget
    {
        private string rootPath;
        private string name;
        private int colSpan;
        private string contentPath;
        private string optionsContentPath;
        private string hubContentPath;
        private string iconPath;
        private WebBrowser browser;
        private int mouseX, mouseY;
        private ContextMenu contextMenu;
        private MenuItem lockItem;
        private MenuItem refreshItem;
        private MenuItem optionsItem;
        private Window optionsWindow;
        private WebBrowser optionsBrowser;
        private HubWindow hub;
        private WebBrowser hubBrowser;
        private DispatcherTimer timer;

        public MosaicHtmlWidget(string root)
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
            get { return browser; }
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

        public override int X
        {
            get { return mouseX; }
        }

        public override int Y
        {
            get { return mouseY; }
        }

        public override void Load()
        {
            browser = new WebBrowser();
            browser.Navigate(rootPath + "\\" + contentPath);
            browser.Navigated += BrowserNavigated;

            contextMenu = new ContextMenu();
            lockItem = new MenuItem();
            lockItem.Header = Properties.Resources.LockItem;
            lockItem.IsCheckable = true;
            contextMenu.Items.Add(lockItem);

            refreshItem = new MenuItem();
            refreshItem.Header = Properties.Resources.RefreshItem;
            refreshItem.Click += RefreshItemClick;
            contextMenu.Items.Add(refreshItem);

            browser.ContextMenu = contextMenu;

            if (!string.IsNullOrEmpty(optionsContentPath))
            {
                optionsItem = new MenuItem();
                optionsItem.Header = "Options";
                optionsItem.Click += new RoutedEventHandler(OptionsItemClick);
                contextMenu.Items.Insert(0, optionsItem);
            }
            if (!string.IsNullOrEmpty(hubContentPath))
            {
                hub = new HubWindow();
            }

            timer = new DispatcherTimer(); //we need to refresh html widgets to resolve rendering bugs when moving widgets
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += new EventHandler(TimerTick);
            timer.Start();
        }


        void TimerTick(object sender, EventArgs e)
        {
            try
            {
                browser.Refresh();
            }
            catch
            {

            }
        }

        void RefreshItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                browser.Refresh();
            }
            catch
            {

            }
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
            optionsBrowser = new WebBrowser();
            optionsBrowser.Navigating += OptionsBrowserNavigating;
            optionsWindow.Content = optionsBrowser;

            if (E.Language == "he-IL" || E.Language == "ar-SA")
            {
                optionsWindow.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                optionsWindow.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            optionsBrowser.Navigate(rootPath + "\\" + optionsContentPath);
            optionsWindow.ShowDialog();
            optionsBrowser.Navigating -= OptionsBrowserNavigating;
            optionsBrowser.Dispose();
            browser.Refresh();
        }

        void OptionsBrowserNavigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.Uri.OriginalString == "javascript:window.close()")
            {
                e.Cancel = true;
                optionsWindow.Close();
            }
        }

        private bool isAssigned;
        HTMLDocumentEvents2_Event iEvent;
        void BrowserNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (browser.Document == null || isAssigned)
                return;
            var document = (HTMLDocument)browser.Document;

            iEvent = (HTMLDocumentEvents2_Event)document;
            iEvent.onclick += EventOnclick;
            iEvent.onmousedown += EventOnmousedown;
            iEvent.onmouseup += EventOnmouseup;
            iEvent.onmousemove += EventOnmousemove;
            iEvent.oncontextmenu += EventOncontextmenu;
            isAssigned = true;
        }

        bool EventOncontextmenu(IHTMLEventObj pEvtObj)
        {
            contextMenu.IsOpen = true;
            return false;
        }

        private int downX, downY;

        void EventOnmousemove(IHTMLEventObj pEvtObj)
        {
            if (lockItem.IsChecked)
                return;

            mouseX = pEvtObj.clientX;
            mouseY = pEvtObj.clientY;
            browser.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount)
            {
                RoutedEvent = Mouse.MouseMoveEvent,
                Source = browser,
            });
        }

        void EventOnmouseup(IHTMLEventObj pEvtObj)
        {
            if (pEvtObj.button == 1 && downX == pEvtObj.clientX && downY == pEvtObj.clientY && hub != null)
            {
                downX = -999;
                downY = -999;
                Clicked();
            }
            if (pEvtObj.button != 1 || lockItem.IsChecked)
                return;
            browser.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = Mouse.MouseUpEvent,
                Source = browser,
            });
        }

        void EventOnmousedown(IHTMLEventObj pEvtObj)
        {
            downX = pEvtObj.clientX;
            downY = pEvtObj.clientY;

            //if (pEvtObj.button == 1)
            //    browser.Refresh();

            if (pEvtObj.button != 1 || lockItem.IsChecked)
                return;
            browser.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = Mouse.MouseDownEvent,
                Source = browser,
            });
        }

        bool EventOnclick(IHTMLEventObj pEvtObj)
        {
            Keyboard.Focus(browser);
            return true;
        }

        public override void Unload()
        {
            timer.Stop();
            if (iEvent != null)
            {
                iEvent.onclick -= EventOnclick;
                iEvent.onmousedown -= EventOnmousedown;
                iEvent.onmouseup -= EventOnmouseup;
                iEvent.onmousemove -= EventOnmousemove;
                iEvent.oncontextmenu -= EventOncontextmenu;
            }

            if (optionsItem != null)
                optionsItem.Click -= OptionsItemClick;

            refreshItem.Click -= RefreshItemClick;

            if (browser != null)
                browser.Dispose();
        }

        private void Clicked()
        {
            if (hub != null && hub.IsVisible)
            {
                hub.Activate();
                return;
            }

            hub = new HubWindow();
            hub.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            hubBrowser = new WebBrowser();
            hubBrowser.Navigating += HubBrowserNavigating;
            hub.Content = hubBrowser;

            if (E.Language == "he-IL" || E.Language == "ar-SA")
            {
                hub.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                hub.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            hubBrowser.Navigate(rootPath + "\\" + hubContentPath);
            hub.ShowDialog();
            hubBrowser.Navigating -= HubBrowserNavigating;
            hubBrowser.Dispose();
        }

        void HubBrowserNavigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.Uri.OriginalString == "javascript:window.close()")
            {
                e.Cancel = true;
                hub.Close();
            }
        }
    }
}
