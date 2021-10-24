using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine.Serialization;


namespace MyRealFrame
{
    [Serializable]
    public class ServerInfo
    {
        [XmlElement("GameVersion")] 
        public VersionInfo[] GameVersion;
    }

    [Serializable]
    public class VersionInfo
    {
        [XmlAttribute] public string Version;
        [FormerlySerializedAs("patches")] [XmlElement] public Patches[] Patches;
    }

    [Serializable]
    public class Patches
    {
        [XmlAttribute] public int Version;
        [XmlAttribute] public string Des;
        [XmlElement] public List<Patch> Files;
    }

    [Serializable]
    public class Patch
    {
        [XmlAttribute]
        public string Name;
        [XmlAttribute]
        public string Url;
        [XmlAttribute]
        public string Platform;
        [XmlAttribute]
        public string Md5;
        [XmlAttribute]
        public float Size;
    }
    
}