
using UnityEditor;
using UnityEngine;

namespace QFrameWork
{
    public partial  class MathUtil
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
        
    

}