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

namespace Mosaic.Base
{
    /// <summary>
    /// Interaction logic for HubWindow.xaml
    /// </summary>
    public partial class HubWindow : Window
    {
        public bool AnimatedOpen = true;

        public HubWindow()
        {
            InitializeComponent();
        }

        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            //this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            if (AnimatedOpen)
            {
                var s = (Storyboard) Resources["LoadAnim"];
                ((DoubleAnimation) s.Children[0]).From = SystemParameters.PrimaryScreenWidth;
                s.Begin(this);
            }
        }

        public void CloseWindow()
        {
            var s = (Storyboard)Resources["UnloadAnim"];
            ((DoubleAnimation)s.Children[0]).To = -SystemParameters.PrimaryScreenWidth;
            s.Begin(this);  
        }

        private void UnloadAnimCompleted(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
