using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace Mosaic
{
    [Serializable]
    public class LoadedWidget //: ISerializable
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name;
        [XmlAttribute(AttributeName = "path")]
        public string Path;
        [XmlAttribute(AttributeName = "id")]
        public string Id;
        [XmlAttribute(AttributeName = "column", DataType = "int")]
        public int Column;
        [XmlAttribute(AttributeName = "row", DataType = "int")]
        public int Row;
    }
}
