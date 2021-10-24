using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MyRealFrame
{
    public class MBuildApp
    {
        private static string m_AppName = PlayerSettings.productName;
        public static string m_AndroidPath = Application.dataPath + "/../BuildTarget/Android/";
        public static string m_IOSPath = Application.dataPath + "/../BuildTarget/IOS/";
        public static string m_WindowsPath = Application.dataPath + "/../BuildTarget/Windows/";


        public static void BuildAssetBundle(bool hotfix = false, string abmd5Path = "", string hotCount = "1")
        {
            DeleteDir(Application.streamingAssetsPath);
            BundleEditor.Build(hotfix, abmd5Path, hotCount);
            string abPath = Application.dataPath + "/../AssetBundle/" +
                            EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
            Copy(abPath, Application.streamingAssetsPath);
        }

        public static void SaveVersion(string version, string package)
        {
            string content = String.Format("Version|{0};PackageName|{1};", version, package);
            string dir = Application.streamingAssetsPath + "/Resources/";
            string savePath = Application.dataPath  + "/Resources/Version.txt";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string oneLine = "";
            string all = "";
            //读取所有的内容
            using (FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                FileShare.ReadWrite))
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
                PlayerSettings.Android.keystorePass = "xxxx";
                PlayerSettings.Android.keyaliasPass = "xxxx";
                PlayerSettings.Android.keyaliasName = "xxxx.keystore";
                PlayerSettings.Android.keystoreName = Application.dataPath.Replace("/Assets", "") + "/realfram.keystore";
                savePath = m_AndroidPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget + string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now) + ".apk";
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                savePath = m_IOSPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget + string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now);
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows|| EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
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
            //生成版本号
            SaveVersion(PlayerSettings.bundleVersion, PlayerSettings.applicationIdentifier);
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
                if (scene.enabled) continue;
                editorScenes.Add(scene.path);
            }

            return editorScenes.ToArray();
        }

        //将指定文件夹路径下的文件拷贝到另一个文件夹
        public static void Copy(string srcPath, string targetPath)
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
                        Copy(file, scrdir);
                    }
                    else
                    {
                        File.Copy(file, Path.Combine(scrdir, Path.GetFileName(file)), true);
                    }
                }

            }
            catch (Exception e)
            {
                Debug.LogError("无法复制：" + srcPath + "  到" + targetPath);
            }
        }

        public static void WriteBuildName(string name)
        {
            FileInfo fi = new FileInfo(Application.dataPath + "/../buildname.txt");
            StreamWriter sw = fi.CreateText();
            sw.WriteLine(name);
            sw.Close();
            sw.Dispose();
        }
        //删除指定文件夹，以及下面的所有文件
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

        public enum Channel
        {
            None = 0,
            Xiaomi,
            Bilibili,
            Huawei,
            Meizu,
            Weixin,
        }

        public class BuildSetting
        {
            //版本号
            public string Version = "";

            //build次数
            public string Build = "";

            //程序名称
            public string Name = "";

            //是否Debug
            public bool Debug = true;

            //渠道
            public Channel Channel = Channel.None;

            //多线程渲染
            public bool MulRendering = true;

            //是否IL2Cpp
            public bool IL2CPP = false;

            //是否开启动态合批
            public bool DynamicBatching = false;

            //是否热更
            public bool IsHotFix = false;

            //对应版本的路径
            public string HotPath = "";

            //热更次数
            public int HotCount = 0;
        }

        #region PC版本打包
        public static void BuildPC()
        {
            //get pc settings 
            BuildSetting buildSetting = GetPCBuildSetting();
            if (buildSetting.IsHotFix)
            {
                BundleEditor.Build(true,buildSetting.HotPath,buildSetting.HotCount.ToString());
                return;
            }
            BundleEditor.Build();
            string suffix = SetPcSetting(buildSetting);
            //生成可执行程序
            string abPath = Application.dataPath + "/../AssetBundle/" +
                            EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
            
            DeleteDir(m_WindowsPath);
            Copy(abPath,Application.streamingAssetsPath);
            string dir = m_AppName + "_PC" + suffix + string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now);
            string name = string.Format("/{0}.exe", m_AppName);
            string savePath = m_WindowsPath + dir + name;
            BuildPipeline.BuildPlayer(FindEnableEditorScenes(), savePath, EditorUserBuildSettings.activeBuildTarget,
                BuildOptions.None);
            //DeleteDir(Application.streamingAssetsPath);
            WriteBuildName(dir);
        }

        static BuildSetting GetPCBuildSetting() 
        {
             string[] parameter = Environment.GetCommandLineArgs();
             BuildSetting buildSetting = new BuildSetting();
             foreach (var str in parameter)
             {
                 if (str.StartsWith("Version"))
                 {
                     var tempParam = str.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
                     if (tempParam.Length == 2)
                     {
                         buildSetting.Version = tempParam[1];
                     }
                 }
                 else if (str.StartsWith("Build"))
                 {
                     var tempParam = str.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
                     if (tempParam.Length == 2)
                     {
                         buildSetting.Build = tempParam[1];
                     }
                 } 
                 else if (str.StartsWith("Name"))
                 {
                     var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                     if (tempParam.Length == 2)
                     {
                         buildSetting.Name = tempParam[1].Trim();
                     }
                 }
                 else if (str.StartsWith("Debug"))
                 {
                     var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                     if (tempParam.Length == 2)
                     {
                         bool.TryParse(tempParam[1], out buildSetting.Debug);
                     }
                 }
                 else if (str.StartsWith("IsHotFix"))
                 {
                     var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                     if (tempParam.Length == 2)
                     {
                         bool.TryParse(tempParam[1], out buildSetting.IsHotFix);
                     }
                 }
                 else if (str.StartsWith("HotVerPath"))
                 {
                     var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                     if (tempParam.Length == 2)
                     {
                         buildSetting.HotPath = tempParam[1].Trim();
                     }
                 }
                 else if (str.StartsWith("HotCount"))
                 {
                     var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                     if (tempParam.Length == 2)
                     {
                         int.TryParse(tempParam[1], out buildSetting.HotCount);
                     }
                 }
                 
             }
             return buildSetting; 
        }

        static string SetPcSetting(BuildSetting setting)
        {
            string suffix = "_";
            if (!string.IsNullOrEmpty(setting.Version))
            {
                PlayerSettings.bundleVersion = setting.Version;
                suffix += setting.Version;
            }

            if (!string.IsNullOrEmpty(setting.Build))
            {
                PlayerSettings.macOS.buildNumber = setting.Build;
                suffix += "_" + setting.Build;
            }

            if (!string.IsNullOrEmpty(setting.Name))
            {
                PlayerSettings.productName = setting.Name;
            }

            if (setting.Debug)
            {
                EditorUserBuildSettings.development = true;
                EditorUserBuildSettings.connectProfiler = true;
                suffix += "_Debug";
            }
            else
            {
                EditorUserBuildSettings.development = false;
            }

            return suffix;
        }
        #endregion

        #region 打包安卓
        public static void BuildAndroid()
        {
            MyRealFrame.MBuildApp.BuildAssetBundle();
            PlayerSettings.Android.keystoreName = "xxxx";
            PlayerSettings.Android.keyaliasPass = "xxxx";
            PlayerSettings.Android.keyaliasPass = "xxxx";
            PlayerSettings.Android.keyaliasName = Application.dataPath.Replace("/Asset","")+ "/realfram.keystore";
            BuildSetting buildSetting = getAndroidBuildSetting();
            string suffix = SetAndroidSetting(buildSetting);
            //生成可执行程序
            string abPath = Application.dataPath + "/../AssetBundle/" + EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
            //清空生成的文件夹
            DeleteDir(m_AndroidPath);
            Copy(abPath, Application.streamingAssetsPath);
            string savePath = m_AndroidPath + m_AppName + "_Andorid" + suffix + string.Format("_{0:yyyy_MM_dd_HH_mm}.apk", DateTime.Now);
            BuildPipeline.BuildPlayer(FindEnableEditorScenes(), savePath, EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);
            DeleteDir(Application.streamingAssetsPath);
        }

        static BuildSetting getAndroidBuildSetting()
        {
            string[] parameters = Environment.GetCommandLineArgs();
            BuildSetting buildSetting = new BuildSetting();
             foreach (string str in parameters)
             {
                    if (str.StartsWith("Channel"))
                    {
                        var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tempParam.Length == 2)
                        {
                            buildSetting.Channel = (Channel)Enum.Parse(typeof(Channel), tempParam[1], true);
                        }
                    }
                    else if (str.StartsWith("Version"))
                    {
                        var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tempParam.Length == 2)
                        {
                            buildSetting.Version = tempParam[1].Trim();
                        }
                    }
                    else if (str.StartsWith("Build"))
                    {
                        var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tempParam.Length == 2)
                        {
                            buildSetting.Build = tempParam[1].Trim();
                        }
                    }
                    else if (str.StartsWith("Name"))
                    {
                        var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tempParam.Length == 2)
                        {
                            buildSetting.Name = tempParam[1].Trim();
                        }
                    }
                    else if (str.StartsWith("Debug"))
                    {
                        var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tempParam.Length == 2)
                        {
                            bool.TryParse(tempParam[1], out buildSetting.Debug);
                        }
                    }
                    else if (str.StartsWith("MulRendering"))
                    {
                        var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tempParam.Length == 2)
                        {
                            bool.TryParse(tempParam[1], out buildSetting.MulRendering);
                        }
                    }
                    else if (str.StartsWith("IL2CPP"))
                    {
                        var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tempParam.Length == 2)
                        {
                            bool.TryParse(tempParam[1], out buildSetting.IL2CPP);
                        }
                    }
             }
             return buildSetting;
        }
        
        
        static string SetAndroidSetting(BuildSetting setting)
        {
            string suffix = "_";
            if (setting.Channel != Channel.None)
            {
                //代表了渠道包
                string symbol = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, symbol + ";" + setting.Channel.ToString());
                suffix += setting.Channel.ToString();
            }

            if (!string.IsNullOrEmpty(setting.Version))
            {
                PlayerSettings.bundleVersion = setting.Version;
                suffix += setting.Version;
            }
            if (!string.IsNullOrEmpty(setting.Build))
            {
                PlayerSettings.Android.bundleVersionCode = int.Parse(setting.Build);
                suffix += "_" + setting.Build;
            }
            if (!string.IsNullOrEmpty(setting.Name))
            {
                PlayerSettings.productName = setting.Name;
                //PlayerSettings.applicationIdentifier = "com.TTT." + setting.Name;
            }

            if (setting.MulRendering)
            {
                PlayerSettings.MTRendering = true;
                suffix += "_MTR";
            }
            else
            {
                PlayerSettings.MTRendering = false;
            }

            if (setting.IL2CPP)
            {
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                suffix += "_IL2CPP";
            }
            else
            {
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            }

            if (setting.Debug)
            {
                EditorUserBuildSettings.development = true;
                EditorUserBuildSettings.connectProfiler = true;
                suffix += "_Debug";
            }
            else
            {
                EditorUserBuildSettings.development = false;
            }
            return suffix;
        }
        
        #endregion

        #region IOS 打包
        public static void BuildIOS()
        {
            MyRealFrame.BundleEditor.Build();
            BuildSetting buildSetting = GetIOSBuildSetting();
            string suffix = SetIOSSetting(buildSetting);
            //生成可执行程序
            string abPath = Application.dataPath + "/../AssetBundle/" + EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
            //清空生成的文件夹
            DeleteDir(m_IOSPath);
            Copy(abPath, Application.streamingAssetsPath);
            string name = m_AppName + "_IOS" + suffix + string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now);
            string savePath = m_IOSPath + name;
            BuildPipeline.BuildPlayer(FindEnableEditorScenes(), savePath, EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);
            DeleteDir(Application.streamingAssetsPath);
            WriteBuildName(name);
        }
        
        static string SetIOSSetting(BuildSetting setting)
        {
            string suffix = "_";

            if (!string.IsNullOrEmpty(setting.Version))
            {
                PlayerSettings.bundleVersion = setting.Version;
                suffix += setting.Version;
            }
            if (!string.IsNullOrEmpty(setting.Build))
            {
                PlayerSettings.iOS.buildNumber = setting.Build;
                suffix += "_" + setting.Build;
            }
            if (!string.IsNullOrEmpty(setting.Name))
            {
                PlayerSettings.productName = setting.Name;
                //PlayerSettings.applicationIdentifier = "com.TTT." + setting.Name;
            }

            if (setting.MulRendering)
            {
                PlayerSettings.MTRendering = true;
                suffix += "_MTR";
            }
            else
            {
                PlayerSettings.MTRendering = false;
            }

            if (setting.DynamicBatching)
            {
                suffix += "_Dynamic";
            }
            else
            {

            }

            return suffix;
        }
        static BuildSetting GetIOSBuildSetting()
        {
            string[] parameters = Environment.GetCommandLineArgs();
            BuildSetting buildSetting = new BuildSetting();
            foreach (string str in parameters)
            {
                if (str.StartsWith("Version"))
                {
                    var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    if (tempParam.Length == 2)
                    {
                        buildSetting.Version = tempParam[1].Trim();
                    }
                }
                else if (str.StartsWith("Build"))
                {
                    var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    if (tempParam.Length == 2)
                    {
                        buildSetting.Build = tempParam[1].Trim();
                    }
                }
                else if (str.StartsWith("Name"))
                {
                    var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    if (tempParam.Length == 2)
                    {
                        buildSetting.Name = tempParam[1].Trim();
                    }
                }
                else if (str.StartsWith("MulRendering"))
                {
                    var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    if (tempParam.Length == 2)
                    {
                        bool.TryParse(tempParam[1], out buildSetting.MulRendering);
                    }
                }
                else if (str.StartsWith("DynamicBatching"))
                {
                    var tempParam = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    if (tempParam.Length == 2)
                    {
                        bool.TryParse(tempParam[1], out buildSetting.DynamicBatching);
                    }
                }
            }
            return buildSetting;
        }
        #endregion
    }
}