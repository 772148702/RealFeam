using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

        [MenuItem("MBuild/构建AB增量包")]
        public static void BuildABExpand()
        {
            MBuildApp.BuildPC();
            string newstring = Application.dataPath + "/Resources/ABMD5.bytes";
            MBuildApp.BuildAssetBundle(true, newstring, "1");
        }

        [MenuItem("MBuild/PC打包")]
        public static void BuildPC()
        {
            MBuildApp.BuildPC();
        }

        [MenuItem("MBuild/标准包")]
        public static void BuildStandardAB()
        {
            MBuildApp.Build();
        }

        [MenuItem("MBuild/资源版本号")]
        public static void BuildVersionNumber()
        {
            MBuildApp.SaveVersion("1.0", "realFrame");
        }

        [MenuItem("MBuild/classToXml")]
        public static void TestClassToXml()
        {
            UnityEngine.Object[] objs = Selection.objects;

            for (int i = 0; i < objs.Length; i++)
            {
                MyRealFrame.MDataEdtior.ClassToXml(objs[i].name);
                EditorUtility.DisplayProgressBar("change to xml", "change to xml", i * 1.0f / objs.Length);
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("MBuild/classToBinary")]
        public static void TestClassToBinary()
        {
            UnityEngine.Object[] objs = Selection.objects;

            for (int i = 0; i < objs.Length; i++)
            {
                MyRealFrame.MDataEdtior.ClassToBinary(objs[i].name);
                EditorUtility.DisplayProgressBar("change to xml", "change to xml", i * 1.0f / objs.Length);
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("MBuild/ABMd5FileDecryption")]
        public static void DeCryptMd5Bytes()
        {
             string ab = Application.dataPath + "/../Version/" + EditorUserBuildSettings.activeBuildTarget.ToString();
             DirectoryInfo _directoryInfo = new DirectoryInfo(ab);
             foreach (var item in _directoryInfo.GetFiles())
             {
                 using (FileStream fs = new FileStream(item.FullName, FileMode.OpenOrCreate, FileAccess.Read,
                     FileShare.ReadWrite))
                 {
                     BinaryFormatter bf = new BinaryFormatter();
                     ABMD5 abmd5 = bf.Deserialize(fs) as ABMD5;
                     Debug.Log(abmd5.ToString());
                 }

             }
             
        }

     }

}