using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace QFrameWork
{
    public abstract partial  class MonoBehaviourSimplify:MonoBehaviour
    {
        public void Delay(float seconds, Action onFinished)
        {
            StartCoroutine(DelayCoroutine(seconds,onFinished));
        }

        private static IEnumerator DelayCoroutine(float seconds, Action cb)
        {
            yield return new WaitForSeconds(seconds);
            cb();
        }

        public void Show()
        {
            GameObjectUtil.Show(gameObject);
        }

        public void Hide()
        {
            GameObjectUtil.Hide(gameObject);
        }

        public void Identity()
        {
            TransformUtil.Identity(transform);
        }
        
    }

  
    
   
}