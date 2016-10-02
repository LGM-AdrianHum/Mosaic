using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mosaic.Base;

namespace Video
{
    public class Widget : MosaicWidget
    {
        private VideoWidget widgetControl;
        //public static Settings Settings;

        public override string Name
        {
            get { return "Video"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return new Uri("/Video;component/Resources/videowidget_icon.png", UriKind.Relative); }
        }

        public override int ColumnSpan
        {
            get { return 1; }
        }

        public override void Load()
        {
            //Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Me\\Me.config") ?? new Settings();
            widgetControl = new VideoWidget();
            //widgetControl.Load();
        }

        public override void Unload()
        {
            //Settings.Save(E.WidgetsRoot + "\\Me\\Me.config");
            //widgetControl.Unload();
        }
    }
}
