

using UnityEditor;
using UnityEngine;

namespace MyRealFram.Editor.Resource
{
    public class MBuildApp
    {
        private static  string m_AppName = PlayerSettings.productName;
        public  static  string m_Android = Application.dataPath + "/../BuildTarget/Android";
        public static string m_IOSPath = Application.dataPath + "/../BuildTarget/IOS";
        public static string m_WindowsPath = Application.dataPath + "/../BuildTarget/Windows";

        [MenuItem("MBuild/标准包")]
        public static void Build()
        {
            BundleEditor.Build();
        }
    }
}