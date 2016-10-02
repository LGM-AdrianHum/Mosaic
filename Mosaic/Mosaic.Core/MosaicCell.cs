using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mosaic.Core
{
    public class MosaicCell
    {
        public int Column;
        public int Row;

        public MosaicCell(int column, int row)
        {
            Column = column;
            Row = row;
        }
    }
}
