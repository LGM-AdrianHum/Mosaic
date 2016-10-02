using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mosaic.Windows
{
    public interface IToolbarWindow
    {
        bool IsOpened { get;}
        void OpenToolbar();
        void CloseToolbar();
    }
}
