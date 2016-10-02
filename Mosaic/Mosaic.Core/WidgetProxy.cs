using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Mosaic.Base;

namespace Mosaic.Core
{
    public class WidgetProxy
    {
        public readonly string Path;
        private Assembly assembly;
        public MosaicWidget WidgetComponent { get; private set; }
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public bool IsLoaded { get; private set; }
        public string Name { get; private set; }
        public bool HasErrors { get; private set; }
        public int Column { get; set; }
        public int Row { get; set; }
        public WidgetType WidgetType { get; set; }

        public WidgetProxy(string path, string name = null, bool isGenerated = false, bool isSocial = false)
        {
            Path = path;
            Column = -1;
            Row = -1;

            if (isGenerated)
            {
                InitializeGenerated();
                return;
            }

            if (isSocial)
            {
                Name = name;
                InitializeSocial();
                return;
            }

            Initialize();
        }

        private void Initialize()
        {
            Type widgetType = null;
            try
            {
                assembly = Assembly.LoadFrom(Path);
                widgetType = assembly.GetTypes().FirstOrDefault(type => typeof(MosaicWidget).IsAssignableFrom(type));
            }
            catch (ReflectionTypeLoadException ex)
            {
                logger.Error("Failed to load provider from " + Path + ".\n" + ex);
                HasErrors = true;
                return;
            }

            if (widgetType == null)
            {
                logger.Error("Failed to find IWeatherProvider in " + Path);
                HasErrors = true;
                return;
            }

            WidgetComponent = Activator.CreateInstance(widgetType) as MosaicWidget;
            if (WidgetComponent == null)
            {
                HasErrors = true;
                return;
            }

            Name = WidgetComponent.Name;
            WidgetType = WidgetType.Native;
        }

        private void InitializeGenerated()
        {
            if (Path.StartsWith("http://"))
                WidgetComponent = new MosaicWebPreviewWidget();
            else
                WidgetComponent = new MosaicAppWidget();
            WidgetType = WidgetType.Generated;
            Name = string.Empty;
        }

        private void InitializeSocial()
        {
            WidgetComponent = new MosaicFriendWidget();
            WidgetType = WidgetType.Generated;
        }


        public void Load()
        {
            if (WidgetType == WidgetType.Generated)
                if (string.IsNullOrEmpty(Name))
                    WidgetComponent.Load(Path);
                else
                    WidgetComponent.Load(Path, Name, Environment.TickCount * Row * Column);
            else
                WidgetComponent.Load();
            IsLoaded = true;
        }

        public void Unload()
        {
            WidgetComponent.Unload();
            IsLoaded = false;
        }
    }
}
