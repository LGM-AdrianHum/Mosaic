using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using Mosaic.Base;

namespace Weather
{
    /// <summary>
    /// Interaction logic for WeatherWidget.xaml
    /// </summary>
    public partial class WeatherWidget : UserControl
    {
        public static WeatherProvider WeatherProvider;
        public static WeatherData CurrentWeather;
        private LocationData currentLocation;
        private Options optionsWindow;
        private DispatcherTimer weatherTimer;
        private DispatcherTimer tileAnimTimer;

        private HubWindow hub;
        private Hub hubContent;

        public WeatherWidget()
        {
            InitializeComponent();
        }

        public void Load()
        {
            WeatherProvider = new WeatherProvider();
            CurrentWeather = (WeatherData)XmlSerializable.Load(typeof(WeatherData), E.WidgetsRoot + "\\Weather\\Weather.data") ?? new WeatherData();
            currentLocation = new LocationData();
            currentLocation.Code = Widget.Settings.LocationCode;
            UpdateWeatherUI();

            weatherTimer = new DispatcherTimer();
            weatherTimer.Interval = TimeSpan.FromMinutes(Widget.Settings.RefreshInterval);
            weatherTimer.Tick += WeatherTimerTick;
            weatherTimer.Start();

            tileAnimTimer = new DispatcherTimer();
            tileAnimTimer.Interval = TimeSpan.FromSeconds(15);
            tileAnimTimer.Tick += TileAnimTimerTick;

            if (!string.IsNullOrEmpty(Widget.Settings.LocationCode))
            {
                RefreshWeather();
                if (E.AnimationEnabled)
                    tileAnimTimer.Start();
            }
            else
            {
                WeatherPanel.Visibility = Visibility.Collapsed;
                Tip.Visibility = Visibility.Visible;
            }
        }

        void TileAnimTimerTick(object sender, EventArgs e)
        {
            var s = (Storyboard)Resources["TileAnim"];
            s.Begin();
        }

        void WeatherTimerTick(object sender, EventArgs e)
        {
            RefreshWeather();
        }

        private void RefreshWeather()
        {
            ThreadStart threadStarter = () =>
            {
                var w = WeatherProvider.GetWeatherReport(CultureInfo.GetCultureInfo(E.Language), currentLocation, Widget.Settings.TempScale);
                if (w != null)
                {
                    CurrentWeather = w;
                    this.Dispatcher.BeginInvoke((Action)UpdateWeatherUI);
                }

                //currentWeather.Save(E.WidgetsRoot + "\\Weather\\Weather.data");
            };
            var thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void UpdateWeatherUI()
        {
            Temperture.Text = CurrentWeather.Temperature + "° " + CurrentWeather.Curent.Text;
            if (CurrentWeather.ForecastList.Count < 3)
                return;
            FirstDay.Text = CurrentWeather.ForecastList[0].Day + ": " + CurrentWeather.ForecastList[0].HighTemperature + "° " + CurrentWeather.ForecastList[0].Text;
            SecondDay.Text = CurrentWeather.ForecastList[1].Day + ": " + CurrentWeather.ForecastList[1].HighTemperature + "° " + CurrentWeather.ForecastList[1].Text;
            ThirdDay.Text = CurrentWeather.ForecastList[2].Day + ": " + CurrentWeather.ForecastList[2].HighTemperature + "° " + CurrentWeather.ForecastList[2].Text;
            WeatherIcon.Source = new BitmapImage(new Uri(string.Format("/Weather;Component/Resources/weather_{0}.png", CurrentWeather.Curent.SkyCode), UriKind.Relative));
            Location.Text = CurrentWeather.Location.City;
        }

        public void Unload()
        {
            CurrentWeather.Save(E.WidgetsRoot + "\\Weather\\Weather.data");
            weatherTimer.Tick -= WeatherTimerTick;
            weatherTimer.Stop();
            tileAnimTimer.Tick -= TileAnimTimerTick;
            tileAnimTimer.Stop();
        }

        private void OptionsItemClick(object sender, RoutedEventArgs e)
        {
            ShowOptions();
        }

        void ShowOptions()
        {
            if (optionsWindow != null && optionsWindow.IsVisible)
            {
                optionsWindow.Activate();
                return;
            }

            optionsWindow = new Options();
            optionsWindow.UpdateSettings += OptionsWindowUpdateSettings;

            if (E.Language == "he-IL" || E.Language == "ar-SA")
            {
                optionsWindow.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                optionsWindow.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            optionsWindow.ShowDialog();
        }

        void OptionsWindowUpdateSettings(object sender, EventArgs e)
        {
            optionsWindow.UpdateSettings -= OptionsWindowUpdateSettings;
            currentLocation.Code = Widget.Settings.LocationCode;

            weatherTimer.Stop();
            weatherTimer.Interval = TimeSpan.FromMinutes(Widget.Settings.RefreshInterval);
            weatherTimer.Start();

            if (!string.IsNullOrEmpty(Widget.Settings.LocationCode))
            {
                Tip.Visibility = Visibility.Collapsed;
                WeatherPanel.Visibility = Visibility.Visible;
                RefreshWeather();
                if (E.AnimationEnabled)
                    tileAnimTimer.Start();
            }
            else
            {
                WeatherPanel.Visibility = Visibility.Collapsed;
                Tip.Visibility = Visibility.Visible;
            }
        }

        private void RefreshItemClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Widget.Settings.LocationCode))
            {
                RefreshWeather();
            }
        }

        private void UserControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(currentLocation.Code))
            {
                ShowOptions();
                return;
            }
            if (hub != null && hub.IsVisible)
            {
                hub.Activate();
                return;
            }

            hub = new HubWindow();
            hub.AnimatedOpen = false;
            hub.Topmost = true;
            hub.AllowsTransparency = true;
            hubContent = new Hub();
            hub.Content = hubContent;
            hubContent.Close += HubContentClose;

            if (E.Language == "he-IL" || E.Language == "ar-SA")
            {
                hub.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            }
            else
            {
                hub.FlowDirection = System.Windows.FlowDirection.LeftToRight;
            }

            hub.ShowDialog();
        }

        void HubContentClose(object sender, EventArgs e)
        {
            hub.CloseWindow();
        }
    }
}
