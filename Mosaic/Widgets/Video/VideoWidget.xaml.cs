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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mosaic.Base;

namespace Video
{
    /// <summary>
    /// Interaction logic for VideoWidget.xaml
    /// </summary>
    public partial class VideoWidget : UserControl
    {
        public VideoWidget()
        {
            InitializeComponent();
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Environment.OSVersion.Version.Major < 6 && Environment.OSVersion.Version.Minor < 1)
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                WinAPI.ShellExecute(IntPtr.Zero, "open", path, null, null, 3);
            }
            else
            {
                var file = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Libraries\\Videos.library-ms";
                WinAPI.ShellExecute(IntPtr.Zero, "open", file, null, null, 3);
            }

        }
    }
}
