using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mosaic.Base;

namespace Weather
{
    public class Widget : MosaicWidget
    {
        private WeatherWidget widgetControl;
        public static Settings Settings;

        public override string Name
        {
            get { return "Weather"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Weather;component/Resources/sun.png", UriKind.Relative); ; }
        }

        public override int ColumnSpan
        {
            get { return 2; }
        }

        public override void Load()
        {
            Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Weather\\Weather.config") ?? new Settings();
            widgetControl = new WeatherWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            Settings.Save(E.WidgetsRoot + "\\Weather\\Weather.config");
            widgetControl.Unload();
        }
    }
}
