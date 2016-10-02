using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mosaic.Base;

namespace Mosaic.Core
{
    public class WidgetManager
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public delegate void WidgetLoadedEventHandler(WidgetProxy widget);
        public delegate void WidgetUnloadedEventHandler(WidgetProxy widget);
        public event WidgetLoadedEventHandler WidgetLoaded;
        public event WidgetUnloadedEventHandler WidgetUnloaded;

        public List<WidgetProxy> Widgets { get; private set; }

        public WidgetManager()
        {
            Widgets = new List<WidgetProxy>();
        }

        public void FindWidgets()
        {
            if (!Directory.Exists(E.WidgetsRoot))
                return;
            var files = from x in Directory.GetDirectories(E.WidgetsRoot)
                        where File.Exists(x + "\\" + Path.GetFileNameWithoutExtension(x) + ".dll")
                        select x + "\\" + Path.GetFileNameWithoutExtension(x) + ".dll";

            foreach (var f in files)
            {
                var w = new WidgetProxy(f);
                //w.Load();
                if (w.HasErrors)
                    continue;
                Widgets.Add(w);
            }

            foreach (var file in Directory.GetFiles(E.WidgetsRoot, "*.bak", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }

            //if (!Directory.Exists(E.AppsRoot))
            //    return;

            //var files = from x in Directory.GetDirectories(E.AppsRoot)
            //            where Directory.GetFiles(x, "*Widget.dll", SearchOption.AllDirectories).Length == 1 /*File.Exists(x + "\\" + Path.GetFileNameWithoutExtension(x) + "\\Widget\\" + ".dll")*/
            //            select Directory.GetFiles(x, "*Widget.dll", SearchOption.AllDirectories).First();


            //foreach (var f in files)
            //{
            //    var w = new WidgetProxy(f);
            //    //w.Load();
            //    if (w.HasErrors)
            //        continue;
            //    Widgets.Add(w);
            //}

            Widgets = Widgets.OrderBy(x => x.Name).ToList();
        }

        public bool IsWidgetLoaded(string name)
        {
            return Widgets.Where(widget => widget.Name == name).Select(widget => widget.IsLoaded).FirstOrDefault();
        }

        public bool HasWidget(string name)
        {
            return Widgets.Any(widget => widget.Name == name);
        }

        public void LoadWidget(string name)
        {
            foreach (var widget in Widgets.Where(widget => widget.Name == name))
            {
                //widget.Load();
                if (WidgetLoaded != null)
                    WidgetLoaded(widget);
            }
        }

        public void LoadExternalWidget(string file, int row = 0, int column = 0)
        {
            if (!File.Exists(file))
                return;
            var w = new WidgetProxy(file);
            if (w.HasErrors)
                return;
            Widgets.Add(w);
            w.Row = row;
            w.Column = column;
            w.Load();
            if (WidgetLoaded != null)
                WidgetLoaded(w);
        }

        public void LoadWidget(WidgetProxy widget)
        {
            if (WidgetLoaded != null)
                WidgetLoaded(widget);

        }

        public void UnloadWidget(string name)
        {
            foreach (var widget in Widgets.Where(widget => widget.Name == name))
            {
                widget.Unload();
                if (WidgetUnloaded != null)
                    WidgetUnloaded(widget);
                break;
            }
        }

        public void UnloadWidget(WidgetProxy widget)
        {
            if (widget.WidgetType == WidgetType.Generated)
                Widgets.Remove(widget);
            widget.Unload();
            if (WidgetUnloaded != null)
                WidgetUnloaded(widget);
        }

        public WidgetProxy CreateWidget(string url)
        {
            var widget = new WidgetProxy(url, null, true);
            Widgets.Add(widget);
            return widget;
        }

        public WidgetProxy CreateFriendWidget(string id, string name)
        {
            var widget = new WidgetProxy(id, name, false, true);
            Widgets.Add(widget);
            return widget;
        }

        //puts widget from source folder to the Mosaic\Widgets folder
        public void InstallWidget(string source, string name)
        {
            if (!Directory.Exists(source))
                return;
            if (!Directory.Exists(E.WidgetsRoot + "\\" + name))
                Directory.CreateDirectory(E.WidgetsRoot + "\\" + name);
            foreach (var file in Directory.GetFiles(source))
            {
                File.Copy(file, E.WidgetsRoot + "\\" + name + "\\" + Path.GetFileName(file));
            }

            if (HasWidget(name))
                return;
            string widgetDll = E.WidgetsRoot + "\\" + name + "\\" + name + ".dll";
            if (File.Exists(widgetDll))
            {
                var w = new WidgetProxy(widgetDll);
                //w.Load();
                if (w.HasErrors)
                    return;
                Widgets.Add(w);
            }
        }

        public void InstallWidgetFromZip(string zipFile, string name)
        {
            if (!File.Exists(zipFile))
            {
                logger.Info("Install widget from zip failed. File " + zipFile + " doesn't exists.");
                return;
            }
            if (!Directory.Exists(E.WidgetsRoot + "\\" + name))
                Directory.CreateDirectory(E.WidgetsRoot + "\\" + name);
            PackageManager.Unpack(zipFile, E.WidgetsRoot + "\\" + name);
            string widgetDll = E.WidgetsRoot + "\\" + name + "\\" + name + ".dll";
            if (File.Exists(widgetDll))
            {
                var w = new WidgetProxy(widgetDll);
                //w.Load();
                if (w.HasErrors)
                    return;
                Widgets.Add(w);
            }
        }

        public WidgetProxy GetWidgetByName(string name)
        {
            return Widgets.Find(widget => widget.Name == name);
        }
    }
}
