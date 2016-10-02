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
using Mosaic.Base;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for DesktopWindow.xaml
    /// </summary>
    public partial class DesktopWindow : Window
    {
        const int WM_COMMAND = 0x111;
        const int MIN_ALL = 419;
        private Point screenOffset;

        public DesktopWindow(Point screenOffset)
        {
            this.screenOffset = screenOffset;

            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            Canvas.SetLeft(DesktopImage, screenOffset.X);
            Canvas.SetTop(DesktopImage, screenOffset.Y);
            DesktopImage.Width = E.MinTileWidth * 2;
            DesktopImage.Height = E.MinTileHeight;

            var s = (Storyboard)Resources["FlyInAnim"];
            ((DoubleAnimation)s.Children[0]).To = SystemParameters.PrimaryScreenWidth;
            ((DoubleAnimation)s.Children[1]).To = SystemParameters.PrimaryScreenHeight;
            s.Begin();
        }

        private void StoryboardCompleted(object sender, EventArgs e)
        {
            Share.SendMessage("Mosaic.Main", "Hide");
            Shell32.Shell objShel = new Shell32.Shell();

            ((Shell32.IShellDispatch4)objShel).ToggleDesktop();

            this.Close();
        }
    }
}
