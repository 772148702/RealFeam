using System;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace QFrameWork
{
    public abstract partial class MonoBehaviourSimplify:MonoBehaviour
    {
        private class MsgRecord
        {
            public string Name;
            public Action<object> OnMsgReceived;
            static Stack<MsgRecord> mMsgRecordPool= new Stack<MsgRecord>();

            public MsgRecord(string name, Action<object> onMsgReceived)
            {
                Name = name;
                OnMsgReceived = onMsgReceived;
            }
            public MsgRecord(){}
            
            public static MsgRecord Allocate(string name, Action<object> onMsgReceived)
            {
                MsgRecord msgRecord;
                msgRecord = mMsgRecordPool.Count > 0 ? mMsgRecordPool.Pop() : new MsgRecord();
                msgRecord.Name = name;
                msgRecord.OnMsgReceived = onMsgReceived;
                return msgRecord;
            }

            public  void Recycle()
            {
                
                mMsgRecordPool.Push(this); 
                Name = null;
                OnMsgReceived = null;
                CommonUtil.PrintLog("recycle, current count is "+mMsgRecordPool.Count); 
            }
        }
        
        //Dictionary<string,Action<object>> mMsgRegisterRecorder = new Dictionary<string, Action<object>>();
        List<MsgRecord> _msgRecords = new List<MsgRecord>();
        protected void RegisterMsg(string msgName, Action<object> onMsgRecevied)
        {
            MsgDispatcher.RegisterMsg(msgName,onMsgRecevied);
            _msgRecords.Add(MsgRecord.Allocate(msgName,onMsgRecevied));
        }

        protected void UnRegister(string msgName)
        {
            var removedMsg = _msgRecords.FindAll(data => data.Name == msgName);

            removedMsg.ForEach(item =>
                {
                    MsgDispatcher.UnRegisterMsg(item.Name, item.OnMsgReceived);
                    _msgRecords.Remove(item);
                    item.Recycle();
                }
            );
            removedMsg.Clear();
        }

        protected void UnRegister(string msgName, Action<object> onMsgRecevie)
        {
            var removedMsg = _msgRecords.FindAll(data => data.Name == msgName&&data.OnMsgReceived==onMsgRecevie);

            removedMsg.ForEach(item =>
                {
                    MsgDispatcher.UnRegisterMsg(item.Name, item.OnMsgReceived);
                    _msgRecords.Remove(item);
                    item.Recycle();
                }
            );
            removedMsg.Clear();
        }
        
        
        private void OnDestroy()
        {
            OnBeforeDestory();
            foreach(var item in _msgRecords)
            {
                MsgDispatcher.UnRegisterMsg(item.Name,item.OnMsgReceived);
                item.Recycle();
            }
            _msgRecords.Clear();
        }

        protected abstract void OnBeforeDestory();
    }
}