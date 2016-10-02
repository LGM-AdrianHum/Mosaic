using System;
using System.Collections.Generic;
using System.IO;
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
using Mosaic.Base;

namespace Weather
{
    /// <summary>
    /// Interaction logic for Hub.xaml
    /// </summary>
    public partial class Hub : UserControl
    {
        public event EventHandler Close;

        public Hub()
        {
            InitializeComponent();
        }

        private void BackButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MediaElement.Stop();
            Close(this, EventArgs.Empty);
        }

        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            CurrentTemp.Text = WeatherWidget.CurrentWeather.Curent.HighTemperature + "°";
            CurrentSky.Text = WeatherWidget.CurrentWeather.Curent.Text;
            CurrentFeelsLike.Text = Properties.Resources.HubFeelsLike + " " + WeatherWidget.CurrentWeather.FeelsLike + "°";
            CurrentLocation.Text = WeatherWidget.CurrentWeather.Location.City;

            for (int i = 0; i < WeatherWidget.CurrentWeather.ForecastList.Count; i++)
            {
                ((ForecastItem)ForecastGrid.Children[i]).Day.Text = DateTime.Now.AddDays(i).ToString("dddd") + " " + DateTime.Now.AddDays(i).Day;
                ((ForecastItem)ForecastGrid.Children[i]).WeatherIcon.Source = new BitmapImage(new Uri(string.Format("/Weather;Component/Resources/weather_{0}.png",
                    WeatherWidget.CurrentWeather.ForecastList[i].SkyCode), UriKind.Relative));
                ((ForecastItem)ForecastGrid.Children[i]).Temperature.Text = WeatherWidget.CurrentWeather.ForecastList[i].HighTemperature + "°";
                ((ForecastItem)ForecastGrid.Children[i]).Sky.Text = WeatherWidget.CurrentWeather.ForecastList[i].Text;
                ((ForecastItem)ForecastGrid.Children[i]).Precipitation.Text = WeatherWidget.CurrentWeather.ForecastList[i].Precipitation + "% " + Properties.Resources.HubChanceOfRain;
                ((ForecastItem)ForecastGrid.Children[i]).LowTemperature.Text = WeatherWidget.CurrentWeather.ForecastList[i].LowTemperature + "° " + Properties.Resources.HubLowTemp;
            }

            for (int i = 0; i < WeatherWidget.CurrentWeather.HourForecastList.Count; i++)
            {
                ((HourForecastItem)HourForecastGrid.Children[i]).Temperature.Text = WeatherWidget.CurrentWeather.HourForecastList[i].Temperature + "°";
                ((HourForecastItem)HourForecastGrid.Children[i]).Time.Text = WeatherWidget.CurrentWeather.HourForecastList[i].Time;
                ((HourForecastItem)HourForecastGrid.Children[i]).ChanceOfRain.Text = WeatherWidget.CurrentWeather.HourForecastList[i].Precipitation + "% " + Properties.Resources.HubChanceOfRain;
            }

            ProviderCopyright.Text = WeatherWidget.CurrentWeather.ProviderCopyright;

            if (Widget.Settings.EnableVideoBackground)
                SetWeatherState(WeatherConverter.ConvertSkyCodeToWeatherState(WeatherWidget.CurrentWeather.Curent.SkyCode));
        }

        private void SetWeatherState(WeatherState state)
        {
            switch (state)
            {
                case WeatherState.Clouds:
                    StartCloudAnimation();
                    break;
                case WeatherState.PartlyCloud:
                    StartPartlyCloudAnim();
                    break;
                case WeatherState.PartlySunny:
                    StartPartlySunnyAnim();
                    break;
                case WeatherState.HeavyRain:
                    StartRainAnim();
                    break;
                case WeatherState.SmallRain:
                    StartRainAnim();
                    break;
                case WeatherState.Storm:
                    StartLightningAnim();
                    break;
                case WeatherState.Clear:
                    StartClearAnim();
                    break;
                case WeatherState.Fog:
                    StartFogAnim();
                    break;
                case WeatherState.Wind:
                    StartWindAnim();
                    break;
            }

            var s = (Storyboard)Resources["ShowVideoAnim"];
            s.Begin();
        }

        private void StartClearAnim()
        {
            if (!File.Exists(E.WidgetsRoot + "\\Weather\\Background\\weather_sunny.mp4"))
                return;
            MediaElement.Source = new Uri(E.WidgetsRoot + "\\Weather\\Background\\weather_sunny.mp4");
            MediaElement.Play();
        }

        private void StartFogAnim()
        {
            if (!File.Exists(E.WidgetsRoot + "\\Weather\\Background\\weather_fog_day.mp4"))
                return;
            MediaElement.Source = new Uri(E.WidgetsRoot + "\\Weather\\Background\\weather_fog_day.mp4");
            MediaElement.Play();
        }

        private void StartWindAnim()
        {
            if (!File.Exists(E.WidgetsRoot + "\\Weather\\Background\\weather_windy_day.mp4"))
                return;
            MediaElement.Source = new Uri(E.WidgetsRoot + "\\Weather\\Background\\weather_windy_day.mp4");
            MediaElement.Play();
        }

        private void StartPartlyCloudAnim()
        {
            if (!File.Exists(E.WidgetsRoot + "\\Weather\\Background\\weather_partly_cloud.mp4"))
                return;
            MediaElement.Source = new Uri(E.WidgetsRoot + "\\Weather\\Background\\weather_partly_cloud.mp4");
            MediaElement.Play();
        }

        private void StartPartlySunnyAnim()
        {
            if (!File.Exists(E.WidgetsRoot + "\\Weather\\Background\\weather_partly_sunny.mp4"))
                return;
            MediaElement.Source = new Uri(E.WidgetsRoot + "\\Weather\\Background\\weather_partly_sunny.mp4");
            MediaElement.Play();
        }


        private void StartCloudAnimation()
        {
            if (!File.Exists(E.WidgetsRoot + "\\Weather\\Background\\weather_cloudy_day.mp4"))
                return;
            MediaElement.Source = new Uri(E.WidgetsRoot + "\\Weather\\Background\\weather_cloudy_day.mp4");
            MediaElement.Play();
        }

        private void StartRainAnim()
        {
            if (!File.Exists(E.WidgetsRoot + "\\Weather\\Background\\weather_rain.mp4"))
                return;
            MediaElement.Source = new Uri(E.WidgetsRoot + "\\Weather\\Background\\weather_rain.mp4");
            MediaElement.Play();
        }

        private void StartLightningAnim()
        {
            if (!File.Exists(E.WidgetsRoot + "\\Weather\\Background\\weather_thunderstorm_day.mp4"))
                return;
            MediaElement.Source = new Uri(E.WidgetsRoot + "\\Weather\\Background\\weather_thunderstorm_day.mp4");
            MediaElement.Play();
        }

        private void MediaElementMediaEnded(object sender, RoutedEventArgs e)
        {
            MediaElement.Position = new TimeSpan();
        }
    }
}
