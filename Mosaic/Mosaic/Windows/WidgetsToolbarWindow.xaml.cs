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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Mosaic.Controls;
using Mosaic.Core;

namespace Mosaic.Windows
{
    /// <summary>
    /// Interaction logic for WidgetsToolbarWindow.xaml
    /// </summary>
    public partial class WidgetsToolbarWindow : Window, IToolbarWindow
    {
        public bool IsOpened { get; private set; }

        public WidgetsToolbarWindow()
        {
            InitializeComponent();

            if (!App.Settings.IsExclusiveMode || App.Settings.ShowTaskbar)
            {
                this.Height = SystemParameters.WorkArea.Height;
                this.Top = SystemParameters.WorkArea.Top;
                this.Opacity = 1;
            }
            else
            {
                this.Height = SystemParameters.PrimaryScreenHeight;
                this.Top = 0;
                this.Opacity = 1;
            }
        }

        private void ToolbarCloseAnimCompleted(object sender, EventArgs e)
        {
            this.Close();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Left = SystemParameters.PrimaryScreenWidth - 1;
        }

        public void OpenToolbar()
        {
            var s = Resources["ToolbarOpenAnim"] as Storyboard;
            ((DoubleAnimation)s.Children[0]).To = SystemParameters.PrimaryScreenWidth - this.Width;

            s.Begin();

            foreach (var w in App.WidgetManager.Widgets)
            {
                if (w.WidgetType == WidgetType.Generated)
                    continue;
                if (w.IsLoaded)
                    continue;
                var item = new ToolbarItem();
                item.Title = w.Name;
                if (w.WidgetComponent.IconPath == null)
                    item.Icon = new BitmapImage(new Uri("/Resources/default_icon.png", UriKind.Relative));
                else
                    item.Icon = new BitmapImage((w.WidgetComponent.IconPath));
                item.MouseLeftButtonDown += ItemMouseLeftButtonDown;
                item.MouseLeftButtonUp += ItemMouseLeftButtonUp;
                WidgetsList.Children.Add(item);
            }
            IsOpened = true;
        }

        public void CloseToolbar()
        {
            var s = Resources["ToolbarCloseAnim"] as Storyboard;
            ((DoubleAnimation)s.Children[0]).To = SystemParameters.PrimaryScreenWidth - 1;
            s.Begin();
            IsOpened = false;
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseToolbar();
        }

        private double mouseX, mouseY;
        void ItemMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseX = e.GetPosition(this).X;
            mouseY = e.GetPosition(this).Y;
        }

        void ItemMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseX != e.GetPosition(this).X || mouseY != e.GetPosition(this).Y)
                return;
            var name = ((ToolbarItem)sender).Title;
            if (App.WidgetManager.IsWidgetLoaded(name))
                App.WidgetManager.UnloadWidget(name);
            else
                App.WidgetManager.LoadWidget(name);
        }
    }
}
