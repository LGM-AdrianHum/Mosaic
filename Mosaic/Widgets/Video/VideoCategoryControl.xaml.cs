using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Video
{
    /// <summary>
    /// Interaction logic for VideoCategoryControl.xaml
    /// </summary>
    public partial class VideoCategoryControl : UserControl
    {

        public VideoCategoryControl()
        {
            InitializeComponent();
        }

        public void Initialize(Category category)
        {
            Header.Text = category.Title.ToString();
            foreach (var file in category.Files)
            {

                var thumbnail = new ThumbnailControl();
                thumbnail.Initialize(file);
                ThumbnailsHost.Children.Add(thumbnail);
            }
        }
    }
}
