using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mosaic.Base
{
    public abstract class MosaicWidget
    {
        public abstract string Name { get; }
        public abstract FrameworkElement WidgetControl { get; }
        public abstract Uri IconPath { get; }
        public abstract int ColumnSpan { get; }

        public virtual void Load() { }
        public virtual void Unload() { }

        public virtual void Load(string path) { }
        public virtual void Load(string id, string name, int seed) { }
        public virtual void Refresh() { }

        public virtual void Notify(string message) {}

        //public virtual int X { get; private set; }
        //public virtual int Y { get; private set; }

        //public abstract event AddContextMenuItemHandler AddContextMenuItem;
        //public abstract event SetBgBrushHandler SetBgBrush;
    }
}
