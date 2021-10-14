using UnityEngine;

namespace QFrameWork
{
    public partial class CommonUtil
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
}