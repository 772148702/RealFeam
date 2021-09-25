

using System.Collections.Generic;
using System.Xml.Serialization;

namespace MyRealFram
{
    [System.Serializable]
    public class AssetBundleConfig
    {
        [XmlElement("ABList")] public List<ABBase> AbBases { get; set; }
    }
    [System.Serializable]
    public class ABBase
    {
        [XmlElement("Path")]
        public string Path { get; set; }
        [XmlElement("Crc")]
        public uint  Crc { get; set; }
        [XmlElement("ABName")]
        public string ABName { get; set; }
        [XmlElement("AssetName")]
        public string AssetName { get; set; }
        [XmlElement("ABDependence")]
        public List<string> ABDependence { get; set; }
    }
}