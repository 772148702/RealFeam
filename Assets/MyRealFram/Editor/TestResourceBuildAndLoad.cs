using UnityEditor;
using UnityEngine;

namespace MyRealFrame
{
    public class TestResourceBuildAndLoad
    {
        [MenuItem("MTools/打包")]
        public static void BuildPkg()
        {
            
            BundleEditor.Build();
        }
        [MenuItem("MBuild/Test/1.加载")]
        public static void ResourceLoadConfigureTest()
        {
            EditorApplication.isPlaying = true;
            GameObject obj = new GameObject();
            var testMonoBehavior = obj.AddComponent<TestMonoBehavior>();
        }
        [MenuItem("MBuild/构建AB")]
        public static void BuildAB()
        {
            MBuildApp.BuildAssetBundle();
        }
        [MenuItem("MBuild/标准包")]
        public static void BuildStandardAB()
        {
            MBuildApp.Build();
        }
        
        [MenuItem("MBuild/资源版本号")]
        public static void BuildVersionNumber()
        {
            MBuildApp.SaveVersion("1.0","realFrame");
        }
        
    }
}