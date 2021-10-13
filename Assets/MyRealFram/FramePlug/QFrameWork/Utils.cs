

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace QFrameWork
{
    public class CommonUtil
    {
        public static void CopyText(string text)
        {
            GUIUtility.systemCopyBuffer = text;
        }

        public static void PrintLog(string content)
        {
            UnityEngine.Debug.Log(content);
        }
          
    }
    public class ExportUtil
    {
        public static void ExportPackage(string path = "Assets/QFramework")
        {
#if UNITY_EDITOR
            AssetDatabase.ExportPackage("Assets/MyRealFram", EditorUtil.GenerateUnityPackageName() + ".unitypackage",ExportPackageOptions.Recurse);
            EditorUtil.OpenFolder();
#endif
      
        }
    }

    public class EditorUtil
    {

        public static string GenerateUnityPackageName()
        {
            string str = "QFrameWork_" + DateTime.Now.ToString("yyyyMMdd_hh");
            Debug.Log(str);
            return str;
        }
        
        public static void OpenFolder(string path=null)
        {
            if (path == null)
            {
                Application.OpenURL("file:///"+ Path.Combine( Application.dataPath,"../"));
            }
            else
            {     
                Application.OpenURL("file:///"+path);
                
            }
            // GUIUtility.systemCopyBuffer = "QFrameWork_" + DateTime.Now.ToString("yyyyMMdd_hh");
        } 
        

        
    }
}