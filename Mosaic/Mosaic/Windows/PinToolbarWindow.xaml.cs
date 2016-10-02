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
using Microsoft.Win32;
using Mosaic.Controls;
using Mosaic.Core;

namespace Mosaic.Windows
{
    /// <summary>
    /// Interaction logic for PinToolbar.xaml
    /// </summary>
    public partial class PinToolbarWindow : Window, IToolbarWindow
    {
        public bool IsOpened { get; private set; }

        public PinToolbarWindow()
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

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Left = SystemParameters.PrimaryScreenWidth - 1;
        }

        public void OpenToolbar()
        {
            var s = Resources["ToolbarOpenAnim"] as Storyboard;
            ((DoubleAnimation)s.Children[0]).To = SystemParameters.PrimaryScreenWidth - this.Width;

            s.Begin();
            IsOpened = true;
        }

        public void CloseToolbar()
        {
            var s = Resources["ToolbarCloseAnim"] as Storyboard;
            ((DoubleAnimation)s.Children[0]).To = SystemParameters.PrimaryScreenWidth - 1;
            s.Begin();
            IsOpened = false;
        }

        private void PinAppButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Executable files|*.exe";
            if (dialog.ShowDialog() != true)
                return;
            CloseToolbar();
            var widget = App.WidgetManager.CreateWidget(dialog.FileName);
            App.WidgetManager.LoadWidget(widget);
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseToolbar();
        }

        private void PinWebButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseToolbar();
            var addressWindow = new AddressBarWindow();
            addressWindow.ShowDialog();
            if (string.IsNullOrEmpty(addressWindow.AddressBox.Text))
                return;
            var widget = App.WidgetManager.CreateWidget(addressWindow.AddressBox.Text);
            App.WidgetManager.LoadWidget(widget);
        }

        private void ToolbarCloseAnimCompleted(object sender, EventArgs e)
        {
            Close();
        }
    }
}
