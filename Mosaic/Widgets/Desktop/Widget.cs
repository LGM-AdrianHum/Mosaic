using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mosaic.Base;

namespace Desktop
{
    public class Widget : MosaicWidget
    {
        private DesktopWidget widgetControl;
        //public static Settings Settings;

        public override string Name
        {
            get { return "Desktop"; }
        }

        public override FrameworkElement WidgetControl
        {
            get { return widgetControl; }
        }

        public override Uri IconPath
        {
            get { return null; }
        }

        public override int ColumnSpan
        {
            get { return 2; }
        }

        public override void Load()
        {
            //Settings = (Settings)XmlSerializable.Load(typeof(Settings), E.WidgetsRoot + "\\Me\\Me.config") ?? new Settings();
            widgetControl = new DesktopWidget();
            widgetControl.Load();
        }

        public override void Unload()
        {
            //Settings.Save(E.WidgetsRoot + "\\Me\\Me.config");
            //widgetControl.Unload();
        }
    }
}
