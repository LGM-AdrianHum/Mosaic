using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mosaic.Base;

namespace Clock
{
    public class Widget : MosaicWidget
    {
        private ClockWidget widgetControl;
        public static Settings Settings;

        public override string Name
        {
            get { return "Clock"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Clock;component/Resources/clock.png", UriKind.Relative); }
        }

        public override int ColumnSpan
        {
            get { return 2; }
        }

        public override void Load()
        {
            Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Clock\\Clock.config") ?? new Settings();
            widgetControl = new ClockWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            Settings.Save(E.WidgetsRoot + "\\Clock\\Clock.config");
            widgetControl.Unload();
        }

        public override void Notify(string message)
        {
            switch (message)
            {
                case "MediaLoaded":
                    widgetControl.HubShowMusic();
                    break;
                case "Play":
                    widgetControl.HubPlay();
                    break;
                case "Pause":
                    widgetControl.HubPause();
                    break;
                case "MediaChanged":
                    widgetControl.HubUpdateMediaInfo();
                    break;
            }
        }
    }
}
