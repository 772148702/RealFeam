using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QFrameWork
{
    public class MsgDispatcher
    {
        private static Dictionary<string, Action<object>> RigesterMesseges = new Dictionary<string, Action<object>>();
        
        public static void RegisterMsg(string name, Action<object> cb)
        {
            if (RigesterMesseges.ContainsKey(name))
            {
                CommonUtil.PrintLog(String.Format("Register {0}, Key is concluded", name));
                RigesterMesseges[name] += cb;
            }
            else
            {
                CommonUtil.PrintLog(String.Format("Register {0}, Key is not concluded", name));
                RigesterMesseges[name] = cb;
            }
        }

        public static  void UnRegisterMsg(string name, Action<object> cb)
        {
            if (!RigesterMesseges.ContainsKey(name))
            {
                CommonUtil.PrintLog(String.Format("The meessage {0} is not registered", name));
                return;
                
            }
            RigesterMesseges[name] -= cb;
            CommonUtil.PrintLog(String.Format("The meessage {0} is being unregistered", name));
        }

        public static void SendMessage(string messageName, object data)
        {
            if (!RigesterMesseges.ContainsKey(messageName))
            {
                CommonUtil.PrintLog(String.Format("The meessage {0} is not registered, but try to invoke", messageName));
                return;
            }

            if (RigesterMesseges[messageName] == null)
            {
                CommonUtil.PrintLog(String.Format("The meessage {0} is registered, but cb is null", messageName));
                return;
            }
            RigesterMesseges[messageName](data);
        }

        public static void UnRegisterAll(string msgName)
        {
            if (RigesterMesseges.ContainsKey(msgName))
            {
                CommonUtil.PrintLog(String.Format("The meessage {0} is unregistered", msgName));
                RigesterMesseges.Remove(msgName);
            }
            else
            {
                CommonUtil.PrintLog(String.Format("The meessage {0} is not registered, but you try to unregister", msgName));
            }
        }
        
        
        
        
#if UNITY_EDITOR
        [MenuItem("QFrameWork/15.消息机制", false, 10)]
#endif
        private static void MenuMsg()
        {
            
            RegisterMsg("test1", fun1);
            RegisterMsg("test1", fun2);
            RegisterMsg("test2", fun1);
            SendMessage("test1","test1 1");
            SendMessage("test2","test2 2");
            UnRegisterMsg("test1",fun1);
            SendMessage("test1","test1 1");
        }
        
        private static void fun1(object test)
        {
            CommonUtil.PrintLog("fun1 "+test.ToString());
            
        }
        
        private static void fun2(object test)
        {
            CommonUtil.PrintLog("fun2 "+test.ToString());
            
        }
    }
}