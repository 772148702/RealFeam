using System;
using UnityEditor;

namespace QFrameWork
{
    public class CommonUtilExample
    {
#if UNITY_EDITOR
        [MenuItem("QFrameWork/Example/1.复制名字到剪切板",false,2)]
#endif
        private static void ToCopyBuffer()
        {
            CommonUtil.CopyText("QFrameWork_" + DateTime.Now.ToString("yyyyMMdd_hh"));
        }
    }
}