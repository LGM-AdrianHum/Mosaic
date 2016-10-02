using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Mosaic.Base;
using Mosaic.Core;

namespace Mosaic.Controls
{
    /// <summary>
    /// Interaction logic for MosaicTile.xaml
    /// </summary>
    public partial class WidgetControl : UserControl
    {
        public readonly WidgetProxy WidgetProxy; private int order = 0;
        public bool MousePressed;
        private ContextMenu contextMenu;
        private MenuItem removeItem;
        private MenuItem refreshItem;

        public int Order
        {
            get { return order; }
            set
            {
                order = value;

                var s = Resources["LoadAnim"] as Storyboard;
                s.BeginTime = TimeSpan.FromMilliseconds(10 + 70 * value);
            }
        }

        public WidgetControl()
        {
            InitializeComponent();
        }

        public WidgetControl(WidgetProxy widgetProxy)
        {
            InitializeComponent();

            this.WidgetProxy = widgetProxy;
        }

        public void Load()
        {
            if (App.Settings.EnableWidgetShadows)
            {
                var shadowEffect = new DropShadowEffect();
                shadowEffect.BlurRadius = 5;
                shadowEffect.ShadowDepth = 3;
                shadowEffect.Opacity = 0.3;
                Effect = shadowEffect;
            }

            FocusManager.SetIsFocusScope(this, true);
            WidgetProxy.Load();
            Root.Children.Clear();
            Root.Children.Add(WidgetProxy.WidgetComponent.WidgetControl);
            this.Width = E.MinTileWidth * WidgetProxy.WidgetComponent.ColumnSpan - E.TileSpacing * 2 * (WidgetProxy.WidgetComponent.ColumnSpan - 1);
            this.Height = E.MinTileHeight - E.TileSpacing * 2;
            this.Margin = new Thickness(E.TileSpacing);
            Grid.SetColumnSpan(this, WidgetProxy.WidgetComponent.ColumnSpan);

            var s = Resources["LoadAnim"] as Storyboard;
            s.Begin();

            if (WidgetProxy.WidgetType == WidgetType.Generated)
            {
                contextMenu = new ContextMenu();

                if (!string.IsNullOrEmpty(WidgetProxy.Path) && WidgetProxy.Path.StartsWith("http://"))
                {
                    refreshItem = new MenuItem();
                    refreshItem.Header = Properties.Resources.RefreshItem;
                    refreshItem.Click += RefreshItemClick;
                    contextMenu.Items.Add(refreshItem);
                }

                removeItem = new MenuItem();
                removeItem.Header = Properties.Resources.RemoveItem;
                removeItem.Click += RemoveItemClick;

                contextMenu.Items.Add(removeItem);
                this.ContextMenu = contextMenu;
            }
        }

        void RefreshItemClick(object sender, RoutedEventArgs e)
        {
            WidgetProxy.WidgetComponent.Refresh();
        }

        void RemoveItemClick(object sender, RoutedEventArgs e)
        {
            removeItem.Click -= RemoveItemClick;
            App.WidgetManager.UnloadWidget(WidgetProxy);
        }

        void WidgetControlMouseMove(object sender, MouseEventArgs e)
        {
            this.RaiseEvent(e);
        }

        void WidgetControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MousePressed = false;
            this.RaiseEvent(e);
        }

        void WidgetControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MousePressed = true;
            this.RaiseEvent(e);
        }

        public void Unload()
        {
            //if (WidgetProxy.WidgetType == WidgetType.Html)
            //{
            //    WidgetProxy.WidgetComponent.WidgetControl.MouseLeftButtonDown -= WidgetControlMouseLeftButtonDown;
            //    WidgetProxy.WidgetComponent.WidgetControl.MouseLeftButtonUp -= WidgetControlMouseLeftButtonUp;
            //    WidgetProxy.WidgetComponent.WidgetControl.MouseMove -= WidgetControlMouseMove;
            //}
            if (refreshItem != null)
                refreshItem.Click -= RefreshItemClick;
            WidgetProxy.Unload();
            Root.Children.Clear();
        }

        private void StoryboardCompleted(object sender, EventArgs e)
        {
            Opacity = 1;
        }

        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var s = Resources["MouseDownAnim"] as Storyboard;
            s.Begin();
            MousePressed = true;
            Keyboard.Focus(this);
            FocusManager.SetFocusedElement(this, this);
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var s = Resources["MouseUpAnim"] as Storyboard;
            s.Begin();
            MousePressed = false;
        }

        private void UserControlMouseLeave(object sender, MouseEventArgs e)
        {
            if (!MousePressed)
                return;
            var s = Resources["MouseUpAnim"] as Storyboard;
            s.Begin();
            MousePressed = false;
        }

    }
}
