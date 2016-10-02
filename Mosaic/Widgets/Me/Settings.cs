using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mosaic.Base;

namespace Me
{
    public class Settings : XmlSerializable
    {
        public Settings()
        {

        }

        public string PicturePath { get; set; }
    }
}
