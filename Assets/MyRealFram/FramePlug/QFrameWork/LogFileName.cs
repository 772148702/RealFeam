using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace QFrameWork
{
    public static class LogFileName
    {

        
#if UNITY_EDITOR
        [MenuItem("QFrameWork/3.生成 unitypackage 名字到剪切板",false,2)]
#endif
        private static void GenerateUnityPackageNameToCopyBuffer()
        {
                CommonUtil.CopyText("QFrameWork_" + DateTime.Now.ToString("yyyyMMdd_hh"));
        }


     #if UNITY_EDITOR
             [MenuItem("QFrameWork/11.Transform归一化")]
     #endif
             private static void MenuIdentity()
             {
                 var trans = new GameObject("transform").transform;
                 TransformUtil.Identity(trans);
             }
#if UNITY_EDITOR
             [MenuItem("QFrameWork/12.AddChild")]
#endif
             private static void MenuAddChild()
             {
                     var transP = new GameObject("Parent").transform;
                     var transC = new GameObject("Child").transform;
                     TransformUtil.AddChild(transP,transC);
                     GameObjectUtil.Hide(transC);
             }       

    }
}