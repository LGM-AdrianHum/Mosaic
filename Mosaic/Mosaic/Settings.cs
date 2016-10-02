using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Mosaic.Base;

namespace Mosaic
{
    public class Settings : XmlSerializable
    {
        public Settings()
        {
            LoadedWidgets = new List<LoadedWidget>();
            Autostart = false;
            Language = CultureInfo.CurrentUICulture.Name;
            IsExclusiveMode = true;
            AnimationEnabled = true;
            ShowGrid = false;
            UseSoftwareRendering = false;
            BackgroundColor = "#22082f";
            EnableThumbnailsBar = Dwm.IsGlassAvailable() && Dwm.IsGlassEnabled();
            ShowMenuButton = true;
            ShowTaskbar = false;
            EnableWidgetShadows = false;
            BackgroundImage = null;
            EnableBackgroundScrolling = true;
            MosaicTitle = "Mosaic";
            EnableStartupAnim = true;
            BackgroundImageOpacity = 1;
        }

        public List<LoadedWidget> LoadedWidgets { get; set; }
        public bool Autostart { get; set; }
        public string Language { get; set; }
        public bool IsExclusiveMode { get; set; }
        public bool AnimationEnabled { get; set; }
        public bool ShowGrid { get; set; }
        public bool UseSoftwareRendering { get; set; }
        public string BackgroundColor { get; set; }
        public bool EnableThumbnailsBar { get; set; }
        public bool ShowMenuButton { get; set; }
        public bool ShowTaskbar { get; set; }
        public bool EnableWidgetShadows { get; set; }
        public string BackgroundImage { get; set; }
        public bool EnableBackgroundScrolling { get; set; }
        public string MosaicTitle { get; set; }
        public bool EnableStartupAnim { get; set; }
        public double BackgroundImageOpacity { get; set; }
    }
}
