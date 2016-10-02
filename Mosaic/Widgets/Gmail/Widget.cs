using System;
using System.Windows;
using Mosaic.Base;

namespace Gmail
{
    public class Widget : MosaicWidget
    {
        private GmailWidget widgetControl;
        public static Settings Settings;

        public override string Name
        {
            get { return "Gmail"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Gmail;component/Resources/gm.png", UriKind.Relative); }
        }

        public override int ColumnSpan
        {
            get { return 2; }
        }

        public override void Load()
        {
            Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Gmail\\Gmail.config") ?? new Settings();
            widgetControl = new GmailWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            Settings.Save(E.WidgetsRoot + "\\Gmail\\Gmail.config");
            widgetControl.Unload();
        }
    }
}
