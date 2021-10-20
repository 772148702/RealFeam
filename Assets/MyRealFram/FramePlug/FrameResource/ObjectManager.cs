using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyRealFram
{
    public class ObjectManager:Singleton<ObjectManager>
    {
        //对象池节点
        public Transform RecyclePoolTrs;
        //场景节点
        public Transform SceneTrs;
        //对象池
        protected Dictionary<uint, List<ResourceObj>> m_ObjectPoolDic = new Dictionary<uint, List<ResourceObj>>();
        //ResourceObj Dic
        protected Dictionary<int, ResourceObj> m_ResourceObjDic = new Dictionary<int, ResourceObj>();
        //ResourceObj类对象池
        protected ClassObjectPool<ResourceObj> m_ResourceObjClassPool = null;
        //根据异步的guid存储ResourceObj，来判断是否正在异步加载
        protected Dictionary<long, ResourceObj> m_AsyncResObjs = new Dictionary<long, ResourceObj>();

        private bool isInit = false;
        //初始化
        public void Init(Transform recyclesTrs =null, Transform sceneTrs=null)
        {
            if (isInit) return;
            isInit = true;
            m_ResourceObjClassPool = GetOrCreateClassPool<ResourceObj>(1000);
            if (recyclesTrs == null)
            {
                var temp = new GameObject();
                temp.name = "recyclesTrs";
                RecyclePoolTrs = temp.transform;
            }
            else
            {
                RecyclePoolTrs = recyclesTrs;
            }

            if (sceneTrs == null)
            {
                var temp = new GameObject();
                temp.name = "recyclesTrs";
                SceneTrs = temp.transform;
            }
            else
            {
                SceneTrs = sceneTrs;
            }
          
        }

        //清除对象池
        public void ClearCache()
        {
            List<uint> tempList = new List<uint>();
            foreach (var key in m_ObjectPoolDic.Keys)
            {
                List<ResourceObj> st = m_ObjectPoolDic[key];
                for (int i = st.Count - 1; i >= 0; i--)
                {
                    ResourceObj resObj = st[i];
                    if (!System.Object.ReferenceEquals(resObj.m_CloneObj, null) && resObj.m_bClear)
                    {
                            GameObject.Destroy(resObj.m_CloneObj);
                            m_ResourceObjDic.Remove(resObj.m_CloneObj.GetInstanceID());
                            resObj.Reset();
                            m_ResourceObjClassPool.Recycle(resObj);
                            st.Remove(resObj);
                    }
                }

                if (st.Count <= 0)
                {
                    tempList.Add(key);
                }
            }

            for (int i = 0; i < tempList.Count; i++)
            {
                uint temp = tempList[i];
                if (m_ObjectPoolDic.ContainsKey(temp))
                {
                    m_ObjectPoolDic.Remove(temp);
                }
            }
            tempList.Clear();
        }
        
        //清空对象池中的某个资源
        public void ClearPoolObject(uint crc)
        {
            List<ResourceObj> st = null;
            if (!m_ObjectPoolDic.TryGetValue(crc, out st) || st == null)
            {
                return;
            }

            for (int i = st.Count - 1; i >= 0; i--)
            {
                ResourceObj resourceObj = st[i];
                if (resourceObj.m_bClear)
                {
                    st.Remove(resourceObj);
                    int tempId = resourceObj.m_CloneObj.GetInstanceID();
                    GameObject.Destroy(resourceObj.m_CloneObj);
                    resourceObj.Reset();
                    m_ResourceObjDic.Remove(tempId);
                    m_ResourceObjClassPool.Recycle(resourceObj);
                }
            }

            if (st.Count <= 0)
            {
                m_ObjectPoolDic.Remove(crc);
            }
        }

        protected ResourceObj GetObjectFromPool(uint crc)
        {
            List<ResourceObj> st = null;
            if (m_ObjectPoolDic.TryGetValue(crc, out st) && st != null && st.Count > 0)
            {
                ResourceManager.Instance.IncreaseResourceRef(crc);
                ResourceObj resourceObj = st[0];
                st.RemoveAt(0);
                GameObject obj = resourceObj.m_CloneObj;
                if (!System.Object.ReferenceEquals(obj, null))
                {
                    if (!System.Object.ReferenceEquals(resourceObj.m_OfflineData, null))
                    {
                        resourceObj.m_OfflineData.ResetProp();
                    }

                    resourceObj.m_Already = false;
#if UNITY_EDITOR
                    if(obj.name.EndsWith("(Recycle)"))
                    {
                        obj.name = obj.name.Replace("(Recycle)", "");
                    }
#endif
                }

                return resourceObj;
            }
            return null;
        }

        public void CancelLoad(long guid)
        {
            ResourceObj resObj = null;
            if (m_AsyncResObjs.TryGetValue(guid, out resObj) && ResourceManager.Instance.CancelLoad(resObj))
            {
                m_AsyncResObjs.Remove(guid);
                resObj.Reset();
                m_ResourceObjClassPool.Recycle(resObj);
            }
        }
        
        //<summary>
        //是否正在异步加载
        public bool IsAsyncLoading(long guid)
        {
            return m_AsyncResObjs[guid] != null;
        }

        public void PreloadGameObject(string path, int count = 1, bool clear = false)
        {
            List<GameObject> tempGameObjectList = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                GameObject obj = InstantiateObject(path, false, clear);
            }
        }
        //同步加载
        public GameObject InstantiateObject(string path, bool setSeceneObj = false, bool bClear = false)
        {
            uint crc = Crc32.GetCrc32(path);
            ResourceObj resourceObj = GetObjectFromPool(crc);
            if (resourceObj == null)
            {
                resourceObj = m_ResourceObjClassPool.Spawn(true);
                resourceObj.m_Crc = crc;
                resourceObj.m_bClear = bClear;
                resourceObj = ResourceManager.Instance.LoadResource(path, resourceObj);

                if (resourceObj.m_ResItem.m_Obj != null)
                {
                    resourceObj.m_CloneObj = GameObject.Instantiate(resourceObj.m_ResItem.m_Obj) as GameObject;
                    resourceObj.m_OfflineData = resourceObj.m_CloneObj.GetComponent<OfflineData>();
                }
            }

            if (setSeceneObj)
            {
                resourceObj.m_CloneObj.transform.SetParent(SceneTrs,false);
            }

            int tempID = resourceObj.m_CloneObj.GetInstanceID();
            if (!m_ResourceObjDic.ContainsKey(tempID))
            {
                m_ResourceObjDic.Add(tempID,resourceObj);
            }

            return resourceObj.m_CloneObj;
        }
        
        //异步加载对象
        public long InstantiateObjAsync(string path, OnAsyncObjectFinish dealFinish, LoadResPriority priority = LoadResPriority.RES_HIGH,
            bool setSceneObject = false, object param1 = null, object param2 = null, object param3 = null,
            bool bClear = true)
        {
            if (string.IsNullOrEmpty(path))
            {
                return 0;
            }

            uint crc = Crc32.GetCrc32(path);
            ResourceObj resObj = GetObjectFromPool(crc);
            if (resObj != null)
            {
                if (setSceneObject)
                {
                    resObj.m_CloneObj.transform.SetParent(SceneTrs,false);
                }

                if (dealFinish != null)
                {
                    dealFinish(path, resObj.m_CloneObj, param1, param2, param3);
                }

                return resObj.m_Guid;
            }

            long guid = ResourceManager.Instance.CreateGuid();
            resObj = m_ResourceObjClassPool.Spawn(true);
            resObj.m_Crc = crc;
            resObj.m_SetSceneParent = setSceneObject;
            resObj.m_bClear = bClear;
            resObj.m_DealObjFinish = dealFinish;
            resObj.m_Param1 = param1;
            resObj.m_Param2 = param2;
            resObj.m_Param3 = param3;
            //调用异步接口
            ResourceManager.Instance.AsyncLoadResource(path,resObj,OnLoadResourceObjFinish,priority);
            return guid;
        }

        void OnLoadResourceObjFinish(string path, ResourceObj resourceObj, object param1 = null, object param2 = null,
            object param3 = null)
        {
            if (resourceObj == null)
                return;
            if (resourceObj.m_ResItem.m_Obj == null)
            {
#if UNITY_EDITOR
                Debug.LogError("异步资源加载的资源为空：" + path);
#endif  
            }
            else
            {
                resourceObj.m_CloneObj = GameObject.Instantiate(resourceObj.m_ResItem.m_Obj) as GameObject;
                resourceObj.m_OfflineData = resourceObj.m_CloneObj.GetComponent<OfflineData>();
            }
            
            //加载完成就从正在加载的异步中移除
            if (m_AsyncResObjs.ContainsKey(resourceObj.m_Guid))
            {
                m_AsyncResObjs.Remove(resourceObj.m_Guid);
            }

            if (resourceObj.m_CloneObj != null && resourceObj.m_SetSceneParent)
            {
                resourceObj.m_CloneObj.transform.SetParent(SceneTrs,false);
            }

            if (resourceObj.m_DealObjFinish != null)
            {
                int tempId = resourceObj.m_CloneObj.GetInstanceID();
                if (!m_ResourceObjDic.ContainsKey(tempId))
                {
                    m_ResourceObjDic.Add(tempId,resourceObj);
                }

                resourceObj.m_DealObjFinish(path, resourceObj.m_CloneObj, resourceObj.m_Param1, resourceObj.m_Param2,
                    resourceObj.m_Param3);
            }
        }

        //没有引用？
        public void ReleaseObject(GameObject obj, int maxCacheCount = -1, bool destoryCache = false,
            bool recycleParent = false)
        {
            if (obj == null) return;
            ResourceObj resourceObj = null;
            int tempID = obj.GetInstanceID();
            if (!m_ResourceObjDic.TryGetValue(tempID, out resourceObj))
            {
                Debug.Log(obj.name + "对象不是ObjectManager创建的！");
                return;
            }

            if (resourceObj == null)
            {
                Debug.LogError("缓存的ResouceObj为空！");
                return;
            }
            if (resourceObj.m_Already)
            {
                Debug.LogError("该对象已经放回对象池了，检测自己是否情况引用!");
                return;
            }
#if UNITY_EDITOR
            obj.name += "(Recycle)";
#endif
            
            List<ResourceObj> st = null;
            if (maxCacheCount == 0)
            {
                m_ResourceObjDic.Remove(tempID);
                ResourceManager.Instance.ReleaseResource(resourceObj, destoryCache);
                resourceObj.Reset();
                m_ResourceObjClassPool.Recycle(resourceObj);
            }
            else
            {
                if (!m_ObjectPoolDic.TryGetValue(resourceObj.m_Crc, out st) || st == null)
                {
                    st = new List<ResourceObj>();
                    m_ObjectPoolDic.Add(resourceObj.m_Crc, st);
                }
                
                if (resourceObj.m_CloneObj)
                {
                    if (recycleParent)
                    {
                        resourceObj.m_CloneObj.transform.SetParent(RecyclePoolTrs);
                    }
                    else
                    {
                        resourceObj.m_CloneObj.SetActive(false);
                    }
                }
                
                if (maxCacheCount < 0 || st.Count < maxCacheCount)
                {
                    st.Add(resourceObj);
                    resourceObj.m_Already = true;
                    //ResourceManager做一个引用计数
                    ResourceManager.Instance.DecreaseResourceRef(resourceObj);
                }
                else
                {
                    //超过缓存的最大量，进行回收资源
                    m_ResourceObjDic.Remove(tempID);
                    ResourceManager.Instance.ReleaseResource(resourceObj, destoryCache);
                    resourceObj.Reset();
                    m_ResourceObjClassPool.Recycle(resourceObj);
                }
            }
        }
        
        #region 类对象池的使用
        protected Dictionary<Type, object> m_ClassPoolDic = new Dictionary<Type, object>();
        public ClassObjectPool<T> GetOrCreateClassPool<T>(int maxcount) where T : class, new()
        {
            Type type = typeof(T);
            object outObj = null;
            if (!m_ClassPoolDic.TryGetValue(type, out outObj) || outObj == null)
            {
                ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxcount);
                m_ClassPoolDic.Add(type,newPool);
                return newPool;
            }

            return outObj as ClassObjectPool<T>;
        }
        
        #endregion
    }
}