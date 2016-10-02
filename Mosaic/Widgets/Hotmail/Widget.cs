using System;
using System.Windows;
using Mosaic.Base;

namespace Hotmail
{
    public class Widget : MosaicWidget
    {
        private HotmailWidget widgetControl;
        public static Settings Settings;

        public override string Name
        {
            get { return "Hotmail"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Hotmail;component/Resources/icon.png", UriKind.Relative); }
        }

        public override int ColumnSpan
        {
            get { return 2; }
        }

        public override void Load()
        {
            Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Hotmail\\Hotmail.config") ?? new Settings();
            widgetControl = new HotmailWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            Settings.Save(E.WidgetsRoot + "\\Hotmail\\Hotmail.config");
            widgetControl.Unload();
        }
    }
}
