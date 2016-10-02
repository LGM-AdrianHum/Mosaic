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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mosaic.Controls
{
    /// <summary>
    /// Interaction logic for ToolbarItem.xaml
    /// </summary>
    public partial class ToolbarItem : UserControl
    {
        private bool mousePressed;
        public ToolbarItem()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return TitleTextBlock.Text; }
            set { TitleTextBlock.Text = value; }
        }

        public ImageSource Icon
        {
            get { return IconImage.Source; }
            set { IconImage.Source = value; }
        }

        private void UserControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mousePressed = true;
            Background = Brushes.Gray;
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mousePressed = false;
            Background = Brushes.Transparent;
        }

        private void UserControlMouseLeave(object sender, MouseEventArgs e)
        {
            if (mousePressed)
            {
                mousePressed = false;
                Background = Brushes.Transparent;
            }
        }

        public void FadeIn(int order)
        {
            Translation.X = 100;
            var s = (Storyboard)Resources["FadeInAnim"];
            s.BeginTime = TimeSpan.FromMilliseconds(order * 30 + 10);
            s.Begin();
        }

        private void StoryboardCompleted(object sender, EventArgs e)
        {
            Translation.X = 0;
        }
    }
}
