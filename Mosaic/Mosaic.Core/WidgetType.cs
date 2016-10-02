using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mosaic.Core
{
    public enum WidgetType
    {
        Native = 0,
        Html = 1,
        Generated = 2 //runtime generated widget such as web thumbnail or app shortcut
    }
}
