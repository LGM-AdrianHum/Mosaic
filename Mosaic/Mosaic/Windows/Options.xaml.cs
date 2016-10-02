using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using Mosaic.Base;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace Mosaic.Windows
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        private readonly List<string> langCodes = new List<string>();
        private bool restartRequired;
        public event EventHandler UpdateSettings;

        public Options()
        {
            InitializeComponent();
        }

        private void WindowSourceInitialized(object sender, EventArgs e)
        {
            this.Top = 0;
            this.Left = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            BuildTag.Text = version + ".beta." + fileInfo.LastWriteTimeUtc.ToString("yyMMdd-HHmm");

            LanguageComboBox.Items.Add(new ComboBoxItem() { Content = CultureInfo.GetCultureInfo("en-US").NativeName });
            langCodes.Add("en-US");
            var langs = from x in Directory.GetDirectories(E.Root) where x.Contains("-") select System.IO.Path.GetFileNameWithoutExtension(x);
            foreach (var l in langs)
            {
                try
                {
                    var c = CultureInfo.GetCultureInfo(l);
                    langCodes.Add(c.Name);
                    LanguageComboBox.Items.Add(new ComboBoxItem() { Content = c.NativeName });
                }
                catch { }
            }

            LanguageComboBox.Text = CultureInfo.GetCultureInfo(App.Settings.Language).NativeName;

            EnableExclusiveCheckBox.IsChecked = App.Settings.IsExclusiveMode;
            EnableAnimationCheckBox.IsChecked = App.Settings.AnimationEnabled;
            //EnableThumbBarCheckBox.IsChecked = App.Settings.EnableThumbnailsBar;
            ShowMenuButtonCheckBox.IsChecked = App.Settings.ShowMenuButton;
            ShowTaskbarCheckBox.IsChecked = App.Settings.ShowTaskbar;
            AutostartCheckBox.IsChecked = App.Settings.Autostart;
            StartupAnimCheckBox.IsChecked = App.Settings.EnableStartupAnim;
            BgImageBox.Text = App.Settings.BackgroundImage;
            ScrollBgCheckBox.IsChecked = App.Settings.EnableBackgroundScrolling;
            MosaicBgColor.Fill = new SolidColorBrush(E.BackgroundColor);
            BgOpacitySlider.Value = App.Settings.BackgroundImageOpacity;

            var s = (Storyboard)Resources["LoadAnim"];
            ((DoubleAnimation)s.Children[0]).From = SystemParameters.PrimaryScreenWidth;
            s.Begin(this);
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            if (restartRequired)
            {
                foreach (Window window in App.Current.Windows)
                {
                    window.Close();
                }

                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            CloseWindow();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void ApplySettings()
        {
            if (App.Settings.IsExclusiveMode != (bool)EnableExclusiveCheckBox.IsChecked)
                restartRequired = true;

            if (App.Settings.ShowTaskbar != (bool)ShowTaskbarCheckBox.IsChecked)
                restartRequired = true;

            App.Settings.IsExclusiveMode = (bool)EnableExclusiveCheckBox.IsChecked;
            App.Settings.AnimationEnabled = (bool)EnableAnimationCheckBox.IsChecked;
            //App.Settings.EnableThumbnailsBar = (bool) EnableThumbBarCheckBox.IsChecked;
            App.Settings.ShowMenuButton = (bool)ShowMenuButtonCheckBox.IsChecked;
            App.Settings.ShowTaskbar = (bool)ShowTaskbarCheckBox.IsChecked;
            App.Settings.Autostart = (bool)AutostartCheckBox.IsChecked;
            App.Settings.EnableStartupAnim = (bool)StartupAnimCheckBox.IsChecked;
            App.Settings.BackgroundImage = BgImageBox.Text;
            App.Settings.EnableBackgroundScrolling = (bool)ScrollBgCheckBox.IsChecked;
            App.Settings.BackgroundImageOpacity = BgOpacitySlider.Value;

            var lastLang = App.Settings.Language;
            if (LanguageComboBox.SelectedIndex >= 0)
                App.Settings.Language = langCodes[LanguageComboBox.SelectedIndex];
            if (!restartRequired)
                restartRequired = lastLang != App.Settings.Language;

            if (App.Settings.Autostart != (bool)AutostartCheckBox.IsChecked)
            {
                App.Settings.Autostart = (bool)AutostartCheckBox.IsChecked;
                if (App.Settings.Autostart)
                {
                    try
                    {
                        using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Run", true))
                        {
                            key.SetValue("Mosaic", "\"" + Assembly.GetExecutingAssembly().Location + "\"", RegistryValueKind.String);
                            key.Close();
                        }
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Run", true))
                        {
                            key.DeleteValue("Mosaic", false);
                            key.Close();
                        }
                    }
                    catch { }
                }
            }

            if (UpdateSettings != null)
            {
                UpdateSettings(null, EventArgs.Empty);
            }

            App.Settings.Save(E.Root + "\\Mosaic.config");
        }


        private void SiteLinkMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WinAPI.ShellExecute(IntPtr.Zero, "open", "http://mosaicproject.codeplex.com", string.Empty, string.Empty, 0);
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseWindow();
        }

        private void ChangeBgColorButtonClick(object sender, RoutedEventArgs e)
        {
            var c = new System.Windows.Forms.ColorDialog();
            if (c.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = Color.FromArgb(E.BackgroundColor.A, c.Color.R, c.Color.G, c.Color.B);
                App.Settings.BackgroundColor = color.ToString();
                E.BackgroundColor = color;
                MosaicBgColor.Fill = new SolidColorBrush(E.BackgroundColor);
                if (App.Settings.IsExclusiveMode)
                {
                    var window = (MainWindow)App.Current.MainWindow;
                    window.Background = new SolidColorBrush(E.BackgroundColor);
                }
            }
        }

        private void CloseWindow()
        {
            var s = (Storyboard)Resources["UnloadAnim"];
            ((DoubleAnimation)s.Children[0]).To = -SystemParameters.PrimaryScreenWidth;
            s.Begin(this);
        }

        private void UnloadAnimCompleted(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BgImageChangeButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Images | *.png;*.jpg;*.bmp";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BgImageBox.Text = dialog.FileName;
            }
        }

        private void BgOpacitySliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BgOpacityValueTextBlock.Text = BgOpacitySlider.Value * 100 + " %";
        }
    }
}
