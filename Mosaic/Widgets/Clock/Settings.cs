using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mosaic.Base;

namespace Clock
{
    public class Settings : XmlSerializable
    {
        public Settings()
        {
            Autolock = true;
            AutolockTime = 300; //seconds
        }

        public bool Autolock { get; set; }
        public int AutolockTime { get; set; }
        public string LockScreenBg { get; set; }
    }
}
