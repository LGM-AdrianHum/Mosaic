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
    /// Interaction logic for BottomToolbar.xaml
    /// </summary>
    public partial class BottomToolbar : UserControl
    {
        public BottomToolbar()
        {
            InitializeComponent();
        }

        public void OpenToolbar()
        {
            var s = (Storyboard)Resources["OpenAnim"];
            s.Begin();
        }

        public void CloseToolbar()
        {
            var s = (Storyboard)Resources["CloseAnim"];
            s.Begin();
        }

        private void CloseAnimCompleted(object sender, EventArgs e)
        {
            ((Canvas)Parent).Children.Remove(this);
        }
    }
}
