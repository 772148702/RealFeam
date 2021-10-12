
using UnityEditor;
using UnityEngine;

namespace QFrameWork
{
    public class MathUtil
    {
        public static bool Percent(int percent)
        {
            return Random.Range(0, 100) < percent;
        }
    }

    public class PreviousClass
    {
#if UNITY_EDITOR
        [MenuItem("QFrameWork/12.概率函数",false,10)]
#endif
        private static void MenuPercent()
        {
            Debug.Log(MathUtil.Percent(50));
        }
    }
}