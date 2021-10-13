
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

        public static T GetRandomValuesFrom<T>(params  T[] paramters)
        {
            return paramters[Random.Range(0, paramters.Length)];
        }
        
    }
        
    
    public class PreviousClass
    {
#if UNITY_EDITOR
        [MenuItem("QFrameWork/12.概率函数", false, 10)]
#endif
        private static void MenuPercent()
        {
            Debug.Log(MathUtil.Percent(50));

        }

#if UNITY_EDITOR
            [MenuItem("QFrameWork/13.从若干值中选择一个",false,10)]
#endif
            private static void RandomChose()
            {
                Debug.Log(MathUtil.GetRandomValuesFrom("sdfds","2321","dsadsa"));
                Debug.Log(MathUtil.GetRandomValuesFrom(1,2,3));
            }
        
    }
}