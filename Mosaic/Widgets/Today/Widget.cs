using System;
using System.Windows;
using Mosaic.Base;

namespace Today
{
    public class Widget : MosaicWidget
    {
        private TodayWidget widgetControl;
        public static Settings Settings;

        public override string Name
        {
            get { return "Today"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Today;component/Resources/today2.png", UriKind.Relative); }
        }

        public override int ColumnSpan
        {
            get { return 2; }
        }

        public override void Load()
        {
            Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Today\\Today.config") ?? new Settings();
            widgetControl = new TodayWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            Settings.Save(E.WidgetsRoot + "\\Today\\Today.config");
            widgetControl.Unload();
        }
    }
}
