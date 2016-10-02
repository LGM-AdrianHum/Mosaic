using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Shell;

namespace Video
{
    public class Thumbnail
    {
        public string Title { get; set; }
        public ImageSource Preview { get; private set; }

        public Thumbnail(string file)
        {
            var shellFile = ShellFile.FromFilePath(file);
            /*if (!string.IsNullOrEmpty(shellFile.Properties.System.Title.Value))
                Title = shellFile.Properties.System.Title.Value;*/
            Title = file;
            Preview = shellFile.Thumbnail.BitmapSource;

            shellFile.Dispose();
        }
    }
}
