using UnityEditor;
using UnityEngine;

namespace QFrameWork
{
    public partial class MonoBehaviourSimplify
    {
        #if UNITY_EDITOR
        [MenuItem("QFrameWork/Example/9定时功能")]
        private static void TestDelayFunction()
        {
            UnityEditor.EditorApplication.isPlaying = true;
            new GameObject("Delay").AddComponent<DelayCoroutine>();
            
        }
        #endif
    }
    
    public class DelayCoroutine:MonoBehaviourSimplify
    {
        private void Awake()
        {
            Delay(1.0f,()=>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            });
        }

        protected override void OnBeforeDestory()
        {
            
        }
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
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
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
        [MenuItem("QFrameWork/Example/10.消息传递")]
        private static void TestDelayFunction()
        {
            UnityEditor.EditorApplication.isPlaying = true;
            new GameObject("Msg").AddComponent<MsgBehaviourTest>();
            
        }
#endif
        
    }
}