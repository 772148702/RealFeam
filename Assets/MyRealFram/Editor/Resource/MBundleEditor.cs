using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEditor;

using UnityEngine;
using FileMode = System.IO.FileMode;

    namespace MyRealFrame
    {
        public class BundleEditor
        {
            private static string m_BundleTargetPath = Application.dataPath+"/../AssetBundle/"+ EditorUserBuildSettings.activeBuildTarget.ToString();

            private static string m_VersionMd5Path = Application.dataPath + "/../Version/" +
                                                     EditorUserBuildSettings.activeBuildTarget.ToString();

            private static string m_FilePrefix = "Hot/" + EditorUserBuildSettings.activeBuildTarget.ToString();
            //current hot fix path 
            private static string m_HotPath = Application.dataPath+"/../Hot/"+EditorUserBuildSettings.activeBuildTarget.ToString();
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
            //store the md5 information
            private static Dictionary<string, ABMD5Base> m_PackMd5 = new Dictionary<string, ABMD5Base>();
            
            public static void Build(bool hotfix=false,string abmd5Path="",string hotCount="1")
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

                if (hotfix)
                {
                    ReadMd5Com(abmd5Path,hotCount);
                }
                else
                {
                    WriteABMD5();
                }
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
                
                
                
            }
            //根据刚刚设置的ab名字得到相应的路径，由此生成相应的item
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
                        
                        Debug.Log("AB包:"+allBundles[i]+" 包含资源文件路径:"+allBundlePath[j]);
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
            
            //生成对于item选项 Disctionary<path,ABName>
            static void WriteData(Dictionary<string,string> resPathDic)
            {
                AssetBundleConfig config = new AssetBundleConfig();
                config.AbBases = new List<ABBase>();
                foreach (var path in resPathDic.Keys)
                {
                    if(!ValidPath(path)) continue;

                    ABBase abBase = new ABBase();
                    abBase.Path = path;
                    abBase.Crc = Crc32.GetCrc32(path);
                    abBase.ABName = resPathDic[path];
                    abBase.AssetName = path.Remove(0, path.LastIndexOf("/") + 1);
                    abBase.ABDependence = new List<string>();
                    string[] resDependce = AssetDatabase.GetDependencies(path);
                    //得到依赖文件的的ABm名字，将其加入到依赖中
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

                string xmlPath = m_BundleTargetPath+ "/AssetBundleConfig.xml";
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
            //ab都在同一级文件？
            static void ReadMd5Com(string abMd5Path, string hotCount)
            {
                m_PackMd5.Clear();
                using (FileStream fs = new FileStream(abMd5Path, FileMode.OpenOrCreate, FileAccess.Read,
                    FileShare.ReadWrite))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    ABMD5 abmd5 = bf.Deserialize(fs) as ABMD5;
                    foreach (var item in abmd5.ABMD5List)
                    {
                        m_PackMd5.Add(item.Name,item);
                    }
                }

                List<string> changeList = new List<string>();
                DirectoryInfo directoryInfo = new DirectoryInfo(m_BundleTargetPath);
                FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
                for (int i = 0; i < fileInfos.Length; i++)
                {
                    if (!fileInfos[i].Name.EndsWith(".meta") && !fileInfos[i].Name.EndsWith(".manifest"))
                    {
                        string name = fileInfos[i].Name;
                        string md5 = MD5Manager.Instance.BuildFileMd5(fileInfos[i].FullName);
                        ABMD5Base abmd5Base = null;
                        if (!m_PackMd5.ContainsKey(name))
                        {
                            changeList.Add(name);
                        }
                        else
                        {
                            if (m_PackMd5.TryGetValue(name, out abmd5Base))
                            {
                                if (abmd5Base.Md5 != md5)
                                {
                                    changeList.Add(name);
                                }
                            }
                        }
                    }
                }
                //to CopyTable
                CopyABAndGenerateXml(changeList,hotCount);
            }
            
            
            //gengerate md5 files, which will be useful in hotfix patches
            static void WriteABMD5()
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(m_BundleTargetPath);
                FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
                ABMD5 abmd5 = new ABMD5();
                abmd5.ABMD5List = new List<ABMD5Base>();
                foreach (var item in fileInfos)
                {
                    if (!item.Name.EndsWith(".cs") && !item.Name.EndsWith(".manifest"))
                    {
                        ABMD5Base abmd5Base = new ABMD5Base();
                        abmd5Base.Name = item.Name;
                        abmd5Base.Md5 = MD5Manager.Instance.BuildFileMd5(item.FullName);
                        abmd5Base.Size = item.Length / 1024.0f;
                        abmd5.ABMD5List.Add(abmd5Base);
                    }
                }

                string ABMD5Path = Application.dataPath + "/Resources/ABMD5.bytes";
                UnityEngine.Debug.Log(String.Format("produce md5 file in {0}",ABMD5Path));
                BinarySerializeOpt.BinarySerialize(ABMD5Path, abmd5);
                
                
                //save the .b in external file
                if (!Directory.Exists(m_VersionMd5Path))
                {
                    Directory.CreateDirectory(m_VersionMd5Path);
                }

                string targetPath = m_VersionMd5Path + "/ABMD5_" + PlayerSettings.bundleVersion + ".bytes";
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }
                UnityEngine.Debug.Log(String.Format("copy md5 file from {0} to {1}",ABMD5Path,targetPath));
                File.Copy(ABMD5Path,targetPath);
                
            }
            
            
            public static void DeleteAllFile(string fullpath)
            {
                if (Directory.Exists(fullpath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(fullpath);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();
                    foreach (var item in fileInfos)
                    {
                        if (item.Name.EndsWith(".meta"))
                        {
                            continue;
                        }

                        File.Delete(item.FullName);
                    }
                }
            }
            
            //todo: expand ab file to multi hierarchy
            static void CopyABAndGenerateXml(List<string> changeList, string hotCount)
            {
                string curPath = m_HotPath + "/" + PlayerSettings.bundleVersion + "/" + hotCount;
                if (!Directory.Exists(curPath))
                {
                    Directory.CreateDirectory(curPath);
                }
                DeleteAllFile(curPath);
                foreach (var str in changeList)
                {
                    if (!str.EndsWith(".manifest"))
                    {
                        File.Copy(m_BundleTargetPath+"/"+str,curPath+"/"+str);
                    }
                }
                //server produce patch
                DirectoryInfo directoryInfo = new DirectoryInfo(m_HotPath);
                FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
                Patches patches = new Patches();
                patches.Version = 1;
                patches.Files = new List<Patch>();
                for (int i = 0; i < files.Length; i++)
                {
                    Patch patch = new Patch();
                    patch.Md5 = MD5Manager.Instance.BuildFileMd5(files[i].FullName);
                    patch.Name = files[i].Name;
                    patch.Size = files[i].Length / 1024.0f;
                    patch.Platform = EditorUserBuildSettings.activeBuildTarget.ToString();
                    patch.Url = HotPatchManager.Instance.ServerAddress + "/"+m_FilePrefix+"/" + PlayerSettings.bundleVersion + "/" + hotCount
                    +"/"+files[i].Name;
                    patches.Files.Add(patch);
                }
                BinarySerializeOpt.XmlSerialize(curPath + "/Patch.xml", patches);
                ServerInfo serverInfo =
                    BinarySerializeOpt.XmlDeserialize( Application.dataPath+"/../Hot" + "/ServerInfo.xml", typeof(ServerInfo)) as ServerInfo;
                serverInfo.GameVersion.Last().Patches[0] = patches;
                BinarySerializeOpt.XmlSerialize( Application.dataPath+"/../Hot" + "/ServerInfo.xml",serverInfo);


            }
            
        }
        
    }