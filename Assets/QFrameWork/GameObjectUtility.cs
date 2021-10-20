using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace QFrameWork
{
    public static  class GameObjectUtility
    {
        public static void Delay(this MonoBehaviour monoBehaviour, float seconds, Action onFinished)
        {
            monoBehaviour.StartCoroutine(DelayCoroutine(seconds,onFinished));
        }

        private static IEnumerator DelayCoroutine(float seconds, Action cb)
        {
            yield return new WaitForSeconds(seconds);
            cb();
        }
        
        public static void Identity(this MonoBehaviour monoBehaviour)
        {
            TransformUtil.Identity(monoBehaviour.transform);
        }
        public static void Show(this MonoBehaviour monoBehaviour)
        {
            monoBehaviour.gameObject.SetActive(true);
        }
        public static void Hide(this MonoBehaviour monoBehaviour)
        {
            monoBehaviour.gameObject.SetActive(false);
        }
    }

  
    
   
}