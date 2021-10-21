

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MyRealFrame
{
    public class MBuildApp
    {
        private static  string m_AppName = PlayerSettings.productName;
        public  static  string m_Android = Application.dataPath + "/../BuildTarget/Android/";
        public static string m_IOSPath = Application.dataPath + "/../BuildTarget/IOS/";
        public static string m_WindowsPath = Application.dataPath + "/../BuildTarget/Windows/";

    
        public static void BuildAssetBundle()
        {
            DeleteDir(Application.streamingAssetsPath);
            BundleEditor.Build();
            string abPath = Application.dataPath + "/../AssetBundle/" +
                            EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
            Copy(abPath, Application.streamingAssetsPath);
        }

        public static void SaveVersion(string version, string package)
        {
            string content = String.Format("Version|{0};PackageName|{1};", version, package);
            string dir = Application.streamingAssetsPath + "/Resources/";
            string savePath = Application.streamingAssetsPath + "/Resources/Version.txt";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string oneLine = "";
            string all = "";
            //读取所有的内容
            using (FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.ReadWrite,FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8))
                {
                    all = sr.ReadToEnd();
                    oneLine = all.Split('\r')[0];
                }
            }
            //替换资源版本号
            using (FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    if (string.IsNullOrEmpty(all))
                    {
                        all = content;
                    }
                    else
                    {
                        all = all.Replace(oneLine, content);
                    }
                    sw.Write(all);
                }
            }
        }
        
        public static string GetSavePath()
        {
            string savePath = "";
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                savePath = m_Android + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget +
                           string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now)+".apk";
            } else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                savePath = m_IOSPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget +
                           string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now);
            } else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ||
                       EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
            {
                savePath = m_WindowsPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget + string.Format("_{0:yyyy_MM_dd_HH_mm}/{1}.exe", DateTime.Now, m_AppName);
            }

            return savePath;
        }
        public static void Build()
        {   
            DeleteDir(Application.streamingAssetsPath);
            //打AB包
            BundleEditor.Build();
            //生成可执行程序
            string abPath = Application.dataPath + "/../AssetBundle/" +
                            EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
            Copy(abPath, Application.streamingAssetsPath);

            string savePath = GetSavePath();
            BuildPipeline.BuildPlayer(FindEnableEditorScenes(), savePath, EditorUserBuildSettings.activeBuildTarget,
                BuildOptions.None);
         
        }

        private static string[] FindEnableEditorScenes()
        {
            List<string> editorScenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if(scene.enabled) continue;
                editorScenes.Add(scene.path);
            }
            return editorScenes.ToArray();
        }
        
        public  static void Copy(string srcPath, string targetPath)
        {
            try
            {
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                string scrdir = Path.Combine(targetPath, Path.GetFileName(srcPath));
                if (!Directory.Exists(scrdir))
                {
                    Directory.CreateDirectory(scrdir);
                }

                string[] files = Directory.GetFileSystemEntries(srcPath);
                foreach (var file in files)
                {
                    if (Directory.Exists(file))
                    {
                        Copy(file,scrdir);
                    }
                    else
                    {
                        File.Copy(file,Path.Combine(scrdir,Path.GetFileName(file)),true);
                    }
                }

            }
            catch (Exception e)
            {
                Debug.LogError("无法复制：" + srcPath + "  到" + targetPath);
            }
        }

        public static void DeleteDir(string scrPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(scrPath);
                FileSystemInfo[] fileInfo = dir.GetFileSystemInfos();
                foreach (var info in fileInfo)
                {
                    if (info is DirectoryInfo)
                    {
                        DirectoryInfo subdir = new DirectoryInfo(info.FullName);
                        subdir.Delete(true);
                    }
                    else
                    {
                        File.Delete(info.FullName);
                    }
                
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}