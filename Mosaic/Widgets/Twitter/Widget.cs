using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mosaic.Base;

namespace Twitter
{
    public class Widget : MosaicWidget
    {
        private TwitterWidget widgetControl;
        public static Settings Settings;

        public override string Name
        {
            get { return "Twitter"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Twitter;component/Resources/t.png", UriKind.Relative); ; }
        }

        public override int ColumnSpan
        {
            get { return 2; }
        }

        public override void Load()
        {
            Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Twitter\\Twitter.config") ?? new Settings();
            widgetControl = new TwitterWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            Settings.Save(E.WidgetsRoot + "\\Twitter\\Twitter.config");
            widgetControl.Unload();
        }
    }
}
