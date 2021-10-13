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
    }

    public class DelayCoroutine:MonoBehaviourSimplify
    {
        private void Awake()
        {
            Delay(1.0f,()=>
            {
                UnityEditor.EditorApplication.isPlaying = false;
            });
        }

        protected override void OnBeforeDestory()
        {
            
        }

#if UNITY_EDITOR
        [MenuItem("QFrameWork/14.测试MonoBehaviour")]
        private static void TestDelayFunction()
        {
            UnityEditor.EditorApplication.isPlaying = true;
            new GameObject("Delay").AddComponent<DelayCoroutine>();
            
        }
        #endif
        
    }
    
    public class MsgBehaviourTest:MonoBehaviourSimplify
    {
        private void Awake()
        {
            Delay(1.0f,()=>
            {
                RegisterMsg("msg1",MsgFun1);
                RegisterMsg("msg1",MsgFun1);
                RegisterMsg("msg2",MsgFun1);
                RegisterMsg("msg2",MsgFun2);
                UnRegister("msg1");
                UnRegister("msg2",MsgFun1);
                MsgDispatcher.SendMessage("msg2","Msg2");
                Destroy(this.gameObject);
                UnityEditor.EditorApplication.isPlaying = false;
            });
        }

        public void MsgFun1(object obj)
        {
            CommonUtil.PrintLog("MsgFun1 "+obj.ToString());
        }
        public void MsgFun2(object obj)
        {
            CommonUtil.PrintLog("MsgFun2 "+obj.ToString());
        }
        
        protected override void OnBeforeDestory()
        {
            
        }

#if UNITY_EDITOR
        [MenuItem("QFrameWork/16.测试MonoBehaviourMsg")]
        private static void TestDelayFunction()
        {
            UnityEditor.EditorApplication.isPlaying = true;
            new GameObject("Delay").AddComponent<MsgBehaviourTest>();
            
        }
#endif
        
    }
}