using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mosaic.Base;

namespace Weather
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class Options : Window
    {
        public event EventHandler UpdateSettings;
        private List<LocationData> locations;
        private LocationData currentLocation;

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

            ShowFeelsLikeCheckBox.IsChecked = Widget.Settings.ShowFeelsLike;
            if (Widget.Settings.TempScale == TemperatureScale.Celsius)
                CelsiusRadioButton.IsChecked = true;

            WeatherIntervalSlider.Value = Widget.Settings.RefreshInterval;
            ShowVideoCheckBox.IsChecked = Widget.Settings.EnableVideoBackground;

            var s = (Storyboard)Resources["LoadAnim"];
            ((DoubleAnimation)s.Children[0]).From = SystemParameters.PrimaryScreenWidth;
            s.Begin(this);
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

        private void SiteLinkMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WinAPI.ShellExecute(IntPtr.Zero, "open", "http://mosaicproject.codeplex.com", string.Empty, string.Empty, 0);
        }

        private void ApplySettings()
        {
            if (currentLocation != null)
            {
                Widget.Settings.LocationCode = currentLocation.Code;
            }

            Widget.Settings.ShowFeelsLike = ShowFeelsLikeCheckBox.IsChecked == true;
            Widget.Settings.TempScale = (bool)CelsiusRadioButton.IsChecked ? TemperatureScale.Celsius : TemperatureScale.Fahrenheit;
            Widget.Settings.EnableVideoBackground = ShowVideoCheckBox.IsChecked == true;

            Widget.Settings.Save(E.WidgetsRoot + "\\Weather\\Weather.config");
            if (UpdateSettings != null)
            {
                UpdateSettings(null, EventArgs.Empty);
            }
        }

        private void SearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (SearchBox.Foreground == Brushes.Gray)
            {
                SearchBox.Text = "";
                SearchBox.FontStyle = FontStyles.Normal;
                SearchBox.Foreground = Brushes.Black;
            }

            if (e.Key == Key.Enter && !string.IsNullOrEmpty(SearchBox.Text) && SearchBox.Text.Length > 2)
            {
                SearchPopup.IsOpen = true;
                ProgressBar.IsIndeterminate = true;
                ProgressBar.Visibility = Visibility.Visible;
                SearchResultBox.Items.Clear();
                var query = SearchBox.Text;
                ThreadStart threadStarter =
                    () => GetLocations(query);
                var thread = new Thread(threadStarter);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        private void SearchBoxIsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                if (string.IsNullOrEmpty(SearchBox.Text))
                {
                    SearchBox.Text = Properties.Resources.OptionsSearchBox;
                    SearchBox.FontStyle = FontStyles.Italic;
                    SearchBox.Foreground = Brushes.Gray;
                }
            }
            else
            {
                if (SearchBox.Text == Properties.Resources.OptionsSearchBox)
                {
                    SearchBox.Text = "";
                    SearchBox.FontStyle = FontStyles.Normal;
                    SearchBox.Foreground = Brushes.Black;
                }
            }
        }

        private void SearchResultBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultBox.SelectedIndex == -1)
                return;
            if (SearchResultBox.SelectedIndex > locations.Count)
                currentLocation = locations[locations.Count - 1];
            else
                currentLocation = locations[SearchResultBox.SelectedIndex];
            SearchBox.Text = currentLocation.City;
            SearchPopup.IsOpen = false;
            SearchResultBox.Items.Clear();
        }

        private void GetLocations(string query)
        {
            locations = WeatherWidget.WeatherProvider.GetLocations(query, CultureInfo.GetCultureInfo(E.Language));
            if (locations != null && locations.Count > 0)
            {
                SearchPopup.Dispatcher.Invoke((Action)delegate
                                                           {
                                                               foreach (var location in locations)
                                                               {


                                                                   SearchResultBox.Items.Add(location);
                                                               }

                                                               ProgressBar.IsIndeterminate = false;
                                                               ProgressBar.Visibility = Visibility.Collapsed;
                                                           });

            }
            else
            {
                SearchPopup.Dispatcher.Invoke((Action)delegate
                                                          {
                                                              SearchPopup.IsOpen = false;
                                                          });
            }
        }

        private void WeatherIntervalSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WeatherIntervalSlider.Value < 60)
            {
                WeatherIntervalValueTextBlock.Text = WeatherIntervalSlider.Value + " " + Properties.Resources.OptionsIntervalMinutes;
            }
            else if (WeatherIntervalSlider.Value == 60)
            {
                WeatherIntervalValueTextBlock.Text = 1 + " " + Properties.Resources.OptionsIntervalHours;
            }
            else
            {
                WeatherIntervalValueTextBlock.Text = string.Format("{0} {1} {2} {3}", Math.Truncate(WeatherIntervalSlider.Value / 60), Properties.Resources.OptionsIntervalHours,
                    Math.Abs(Math.IEEERemainder(WeatherIntervalSlider.Value, 60)), Properties.Resources.OptionsIntervalMinutes);
            }
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseWindow();
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

        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SearchBox.Text) && SearchBox.Text.Length > 2)
            {
                SearchPopup.IsOpen = true;
                ProgressBar.IsIndeterminate = true;
                ProgressBar.Visibility = Visibility.Visible;
                SearchResultBox.Items.Clear();
                var query = SearchBox.Text;
                ThreadStart threadStarter =
                    () => GetLocations(query);
                var thread = new Thread(threadStarter);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }
    }
}
