
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MyRealFrame
{
    [System.Serializable]
    public class ABMD5
    {
        public List<ABMD5Base> ABMD5List { get; set; }
    }

    [System.Serializable]
    public class ABMD5Base
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlAttribute("Md5")]
        public string Md5 { get; set; }
        [XmlAttribute("Size")]
        public float Size { get; set; }
    }
}