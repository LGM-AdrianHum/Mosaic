using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Mosaic.Base;

namespace Mosaic.Core
{
    public class ScreenManager
    {
        private Rect region;
        public MosaicMatrix Matrix { get; private set; }

        public Rect Region { get { return region; } }
        
        public void Initialize()
        {
            region = new Rect();

            region.Height = SystemParameters.PrimaryScreenHeight - E.Margin.Top - E.Margin.Bottom;
            region.Width = SystemParameters.PrimaryScreenWidth;
            E.RowsCount = (int)(region.Height / (E.MinTileHeight - E.TileSpacing * 2));
            //E.MinTileHeight = region.Height / E.RowsCount;
            //E.MinTileWidth = E.MinTileHeight;
            region.Y = E.Margin.Top;
            region.X = E.Margin.Left;
            E.ColumnsCount = (int)Math.Round(region.Width * 2 / E.MinTileWidth);

            Matrix = new MosaicMatrix(E.ColumnsCount, E.RowsCount);
            Matrix.ZeroMatrix();
            Matrix[1, 0] = 1; //reserve cell for Mosaic title
        }

        public static BitmapSource GetScreenShot(int x, int y, int width, int height)
        {

            Bitmap screen = new Bitmap(width, height);

            Graphics g = Graphics.FromImage(screen);

            g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));

            g.Dispose();

            return ConvertImage(screen);

        }

        private static BitmapSource ConvertImage(Bitmap image)
        {

            if (image == null)
            {

                return null;

            }

            return Imaging.CreateBitmapSourceFromHBitmap(image.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

        }
    }
}
