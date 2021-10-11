using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName ="MABConfig",menuName = "MCreatABConfig",order = 0)]
public class MABConfig: ScriptableObject
{   
    //文件夹路径
    public List<string> m_AllPrefabPath = new List<string>();
    public List<FileDirABName> m_AllFileDirAB = new List<FileDirABName>();
    [System.Serializable]
    public struct FileDirABName
    {
        public string ABName;
        public string Path;
    }
}
