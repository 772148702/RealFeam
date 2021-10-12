using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace QFrameWork
{
    public static class LogFileName
    {

        
#if UNITY_EDITOR
        [MenuItem("QFrameWork/3.生成 unitypackage 名字到剪切板")]
#endif
        private static void GenerateUnityPackageNameToCopyBuffer()
        {
                CommonUtil.CopyText("QFrameWork_" + DateTime.Now.ToString("yyyyMMdd_hh"));
        }
#if UNITY_EDITOR
        [MenuItem("QFrameWork/5.生成Package")]
#endif
        private static void MenuClick()
        {
                ExportUtil.ExportPackage();
                // GUIUtility.systemCopyBuffer = "QFrameWork_" + DateTime.Now.ToString("yyyyMMdd_hh");
        }      
        
#if UNITY_EDITOR
        [MenuItem("QFrameWork/7.MenuItem复用 %e")]
#endif
        private static void MenuReuseKey()
        {
            EditorApplication.ExecuteMenuItem("QFrameWork/5.打开所在文件夹");
        }
        
        
    }
}