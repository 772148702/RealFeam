

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace MyRealFram.Editor.Resource
{
    public class MBundleEditor
    {
        private static string m_BundleTargetPath = Application.dataPath+"../AssetBundle"+ EditorUserBuildSettings.activeBuildTarget.ToString();
        private static string ABCONFIGPATH = "Assets/MyRealFram/Editor/Resource/ABConfig.asset";
        private static string ABBYTEPATH = MRealConfig.GetRealFram().abPath;
        //key是ab包名,value是路径，所有文件夹ab包dic
        private static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();
        //过滤list
        private static List<string> m_AllFileAB = new List<string>();
        //单个prefab的包,prefab以及其依赖的路径
        private static Dictionary<string, List<string>> m_AllPrefabDir = new Dictionary<string, List<string>>();
        //储存所有的有效路径
        private static List<string> m_ConfigFil = new List<string>();

        [MenuItem("MTools/打包")]
        public static void Build()
        {
            DataEditor.AllExcelToXml();
            m_ConfigFil.Clear();
            m_AllFileAB.Clear();
            m_AllFileDir.Clear();
            m_AllPrefabDir.Clear();
            MABConfig mabConfig = AssetDatabase.LoadAssetAtPath<MABConfig>(ABCONFIGPATH);

            foreach (var item in mabConfig.m_AllFileDirAB)
            {
                if (m_AllFileDir.ContainsKey(item.ABName))
                {
                    UnityEngine.Debug.LogError("AB包配置名字重复，请检查");
                }
                else
                {
                    m_AllFileDir.Add(item.ABName,item.Path);
                    m_AllFileAB.Add(item.Path);
                    m_ConfigFil.Add(item.Path);
                }
            }

            string[] allStr = AssetDatabase.FindAssets("t:Prefab", mabConfig.m_AllPrefabPath.ToArray());
            for (int i = 0; i < allStr.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
                EditorUtility.DisplayProgressBar("查找Prefab", "Prefab:" + path, 1.0f / allStr.Length * i);
                m_ConfigFil.Add(path);
                if (!ContainAllFileAB(path))
                {
                    GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    string[] allDepend = AssetDatabase.GetDependencies(path);
                    List<string> allDependPath = new List<string>();
                    for (int j = 0; j < allDepend.Length; j++)
                    {
                        if (!ContainAllFileAB(allDepend[j]) && !allDepend[j].EndsWith(".cs"))
                        {
                            m_AllFileAB.Add(allDepend[j]);
                            allDependPath.Add(allDepend[j]);
                        }

                        if (m_AllPrefabDir.ContainsKey(obj.name))
                        {
                            Debug.LogError("存在相同名字的prefab,名字:"+obj.name);
                        }
                        else
                        {
                            m_AllPrefabDir.Add(obj.name,allDependPath);
                        }
                    }
                }
            }
            //大包的名字
            foreach (var name in m_AllFileDir.Keys)
            {
                SetABName(name,m_AllFileDir[name]);
            }
            //打包依赖小包的名字
            foreach (var name in m_AllPrefabDir.Keys)
            {
                SetABName(name,m_AllPrefabDir[name]);
            }
            //双向依赖构成了图又会怎么样子呢？好奇
            
            
        }

        static void BuildAssetBundle()
        {
            //设置了名字后会返回
            string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
            Dictionary<string, string> resDic = new Dictionary<string, string>();
            for (int i = 0; i < allBundles.Length; i++)
            {
                string[] allBundlePath = AssetDatabase.GetAssetPathsFromAssetBundle(allBundles[i]);
                
            }
        }
        static void SetABName(string name, string path)
        {
            AssetImporter assetImporter = AssetImporter.GetAtPath(path);
            if (assetImporter == null)
            {
                Debug.LogError("不存在此路径"+path);
            }
            else
            {
                assetImporter.assetBundleName = name;
            }
        }

        static void SetABName(string name, List<string> paths)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                SetABName(name,paths[i]);
            }
        }
        static bool ContainAllFileAB(string path)
        {
            for (int i = 0; i < m_AllFileAB.Count; i++)
            {
                if (path == m_AllFileAB[i] || (path.Contains(m_AllFileAB[i]))
                    && (path.Replace(m_AllFileAB[i], "")[0] == '/'))
                {
                    return true;
                }

         
            }
            return false;
        }
    }
    
}