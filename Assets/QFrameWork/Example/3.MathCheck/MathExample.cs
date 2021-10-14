using UnityEditor;
using UnityEngine;

namespace QFrameWork
{
    public partial  class MathUtil
    {
     
#if UNITY_EDITOR
        [MenuItem("QFrameWork/Example/7.概率函数", false, 10)]
#endif
        private static void MenuPercent()
        {
            Debug.Log(MathUtil.Percent(50));

        }

#if UNITY_EDITOR
            [MenuItem("QFrameWork/Example/8.从若干值中选择一个",false,10)]
#endif
            private static void RandomChose()
            {
                Debug.Log(MathUtil.GetRandomValuesFrom("sdfds","2321","dsadsa"));
                Debug.Log(MathUtil.GetRandomValuesFrom(1,2,3));
            }
    }
}