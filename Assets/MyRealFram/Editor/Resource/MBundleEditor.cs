﻿

    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml.Serialization;
    using UnityEditor;
    using UnityEditor.VersionControl;
    using UnityEngine;
    using FileMode = System.IO.FileMode;

    namespace MyRealFram.Editor.Resource
    {
        public class MBundleEditor
        {
            private static string m_BundleTargetPath = Application.dataPath+"/../AssetBundle"+ EditorUserBuildSettings.activeBuildTarget.ToString();
            private static string ABCONFIGPATH = "Assets/MyRealFram/Editor/Resource/MABConfig.asset";
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
                EditorUtility.ClearProgressBar();
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
                    //prefab路径不包含之前指定的AB包
                    if (!ContainAllFileAB(path))
                    {
                        GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        string[] allDepend = AssetDatabase.GetDependencies(path);
                        List<string> allDependPath = new List<string>();
                        //prefab的依赖不包含
                        for (int j = 0; j < allDepend.Length; j++)
                        {
                            if (!ContainAllFileAB(allDepend[j]) && !allDepend[j].EndsWith(".cs"))
                            {
                                m_AllFileAB.Add(allDepend[j]);
                                allDependPath.Add(allDepend[j]);
                            }
                        }
                        //prefab本身被记录一次
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
                //大包的名字
                foreach (var name in m_AllFileDir.Keys)
                {
                    SetABName(name,m_AllFileDir[name]);
                }
                //打包依赖小包的名字 (小包也设置大包的名字)
                foreach (var name in m_AllPrefabDir.Keys)
                {
                    SetABName(name,m_AllPrefabDir[name]);
                }
                //一个prefab被多个不同的AB包依赖会怎么样呢
                BuildAssetBundle();
                string[] oldNames = AssetDatabase.GetAllAssetBundleNames();
                for (int i = 0; i < oldNames.Length; i++)
                {
                    AssetDatabase.RemoveAssetBundleName(oldNames[i],true);
                    EditorUtility.DisplayProgressBar("清除AB包名字","名字: "+oldNames[i],i*1.0f/oldNames.Length);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
            }

            static void BuildAssetBundle()
            {
                //设置了名字后会返回，也就是说只有在MABCongfigure中的文件才会被记录，被AB命名
                string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
                //路径-AB包的路径
                Dictionary<string, string> resDic = new Dictionary<string, string>();
                for (int i = 0; i < allBundles.Length; i++)
                {
                    string[] allBundlePath = AssetDatabase.GetAssetPathsFromAssetBundle(allBundles[i]);
                    for (int j = 0; j < allBundlePath.Length; j++)
                    {
                        if(allBundlePath[j].EndsWith(".cs")) continue;
                        
                        Debug.Log("此AB包:"+allBundles[i]+"下面包含资源文件路径:"+allBundlePath[j]);
                        resDic.Add(allBundlePath[j],allBundles[i]);
                    }
                }

                if (!Directory.Exists(m_BundleTargetPath))
                {
                    Directory.CreateDirectory(m_BundleTargetPath);
                }
                DeleteAB();
                WriteData(resDic);

                AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(m_BundleTargetPath,
                    BuildAssetBundleOptions.ChunkBasedCompression,
                    EditorUserBuildSettings.activeBuildTarget);
                if (manifest == null)
                {
                    Debug.LogError("AssetBundle 打包失败");
                }
                else
                {
                     Debug.Log("AssetBundle 打包完毕");
                }
            }

            static bool ValidPath(string path)
            {
                for (int i = 0; i < m_ConfigFil.Count; i++)
                {
                    if (path.Contains(m_ConfigFil[i]))
                    {
                        return true;
                    }
                }

                return false;
            }
            
            
            static void WriteData(Dictionary<string,string> resPathDic)
            {
                MyRealFram.AssetBundleConfig config = new MyRealFram.AssetBundleConfig();
                config.AbBases = new List<ABBase>();
                foreach (var path in resPathDic.Keys)
                {
                    if(!ValidPath(path)) continue;

                    MyRealFram.ABBase abBase = new MyRealFram.ABBase();
                    abBase.Path = path;
                    abBase.Crc = Crc32.GetCrc32(path);
                    abBase.ABName = resPathDic[path];
                    abBase.AssetName = path.Remove(0, path.LastIndexOf("/") + 1);
                    abBase.ABDependence = new List<string>();
                    string[] resDependce = AssetDatabase.GetDependencies(path);
                    for (int i = 0; i < resDependce.Length; i++)
                    {
                        string tempPath = resDependce[i];
                        if(tempPath==path||path.EndsWith(".cs")) continue;

                        string abName = "";
                        if (resPathDic.TryGetValue(tempPath, out abName))
                        {
                            if(abName==resPathDic[path]) continue;

                            if (!abBase.ABDependence.Contains(abName))
                            {
                                abBase.ABDependence.Add(abName);
                            }
                        }
                    }
                    config.AbBases.Add(abBase);
                }

                string xmlPath = Application.dataPath + "/AssetBundleConfig.xml";
                if(File.Exists(xmlPath)) File.Delete(xmlPath);

                FileStream fileStream =
                    new FileStream(xmlPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                StreamWriter sw = new StreamWriter(fileStream,System.Text.Encoding.UTF8);
                XmlSerializer xs = new XmlSerializer(config.GetType());
                xs.Serialize(sw,config);
                sw.Close();
                fileStream.Close();

                foreach (var abBase in config.AbBases)
                {
                    abBase.Path = "";
                }

                FileStream fs = new FileStream(ABBYTEPATH, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                fs.Seek(0, SeekOrigin.Begin);
                fs.SetLength(0);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs,config);
                fs.Close();
                AssetDatabase.Refresh();
                SetABName("assetbundleconfig",ABBYTEPATH);
            }
            static void DeleteAB()
            {
                string[] allBundlesName = AssetDatabase.GetAllAssetBundleNames();
                DirectoryInfo directoryInfo = new DirectoryInfo(m_BundleTargetPath);
                FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    if (ContainABName(files[i].Name, allBundlesName) || files[i].Name.EndsWith(".meta") ||
                        files[i].Name.EndsWith(".manifest") ||
                        files[i].Name.EndsWith("assetbundleconfig"))
                    {
                        continue;
                    }
                    else
                    {
                        Debug.Log("此AB已被删除或者改名了："+files[i].Name);
                        if (File.Exists(files[i].FullName))
                        {
                            File.Delete(files[i].FullName);
                        }

                        if (File.Exists(files[i].FullName + ".mainfest"))
                        {
                            File.Delete(files[i].FullName + ".mainfest");
                        }
                    }
                }
                
            }

            static bool ContainABName(string name, string[] strs)
            {
                for (int i = 0; i < strs.Length; i++)
                {
                    if (name == strs[i])
                    {
                        return true;
                    }
                }

                return false;
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
                    if (assetImporter.name != "")
                    {
                        UnityEngine.Debug.LogError("assetImport Name is not null");
                    }
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