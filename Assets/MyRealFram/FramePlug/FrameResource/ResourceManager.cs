﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MyRealFram
{

    public enum LoadResPriority
    {
        RES_HIGH = 0,
        RES_MIDDLE,
        RES_SLOW,
        RES_NUM,
    }

    //在resourceitem上面包裹了一层
    public class ResourceObj
    {
        public uint m_Crc = 0;

        public ResourceItem m_ResItem = null;

        public GameObject m_CloneObj = null;

        public bool m_bClear = true;

        public long m_Guid = 0;

        public bool m_Already = false;

        public bool m_SetSceneParent = false;

        public OnAsyncFinsih m_DealFinish = null;

        public object m_Param1, m_Param2, m_Param3 = null;

        public OfflineData m_OfflineData = null;
        
        public void Reset()
        {
            m_Crc = 0;
            m_CloneObj = null;
            m_bClear = true;
            m_Guid = 0;
            m_ResItem = null;
            m_Already = false;
            m_SetSceneParent = false;
            m_DealFinish = null;
            m_Param1 = m_Param2 = m_Param3 = null;
            m_OfflineData = null;
        }
    }


    public class AsyncLoadResParam
    {
        public List<AsyncCallBack> m_CallBackList = new List<AsyncCallBack>();
        public uint m_Crc;
        public string m_Path;
        public bool m_Sprite = false;
        public LoadResPriority m_Priority = LoadResPriority.RES_SLOW;

        public void Reset()
        {
            m_CallBackList.Clear();
            m_Crc = 0;
            m_Path = "";
            m_Sprite = false;
            m_Priority = LoadResPriority.RES_SLOW;
        }
    }

    public class AsyncCallBack
    {
        public OnAsyncFinsih m_DealFinish = null;

        public ResourceObj m_ResObj = null;

        public OnAsyncObjFinish m_DealObjFinish = null;

        public object m_Param1 = null, m_Param2 = null, m_Param3 = null;

        public void Reset()
        {
            m_DealObjFinish = null;
            m_DealFinish = null;
            m_Param1 = null;
            m_Param2 = null;
            m_Param3 = null;
        }
    }
    //资源加载完成回调
    public delegate void OnAsyncObjFinish(string path, Object obj, object param1 = null, object param2 = null,
        object param3 = null);
    
    //实例化对象完成回调
    public delegate void OnAsyncFinsih(string path, ResourceObj resouceObj, object param1 = null, object param2 = null,
        object param3 = null);
    public class ResourceManager:Singleton<ResourceManager>
    {
        protected long m_Guid = 0;
        public bool m_LoadFromAssetBundle = true;
        //缓存的资源列表
        public Dictionary<uint,ResourceItem> AssetDic { get; set; } = new Dictionary<uint, ResourceItem>();
        //缓存引用引用为零的资源列表，达到醉倒缓存的时候释放最早没用的资源
        protected  CMapList<ResourceItem> m_NoRefrenceAssetMapList = new CMapList<ResourceItem>();
        
        //中间类，回调类对象池
        protected ClassObjectPool<AsyncLoadResParam> m_AsyncLoadResParamPool = new ClassObjectPool<AsyncLoadResParam>(50);
        protected ClassObjectPool<AsyncCallBack> m_AsyncCallBackPool = new ClassObjectPool<AsyncCallBack>(100);
        
        //Mono脚本
        protected MonoBehaviour m_Startmono;
        //正在异步加载的资源列表
        protected  List<AsyncLoadResParam> [] m_LoadingAssetList = new List<AsyncLoadResParam>[(int) LoadResPriority.RES_NUM];
        //正在异步加载的Dic
        protected  Dictionary<uint,AsyncLoadResParam> m_LoadingAssetDic = new Dictionary<uint, AsyncLoadResParam>();
        
        private const long MAX_LOAD_RES_TIME = 20000;
        private const int MAX_CACHE_COUNT = 500;

        public void Init(MonoBehaviour mono)
        {
            for (int i = 0; i < (int) LoadResPriority.RES_NUM; i++)
            {
                m_LoadingAssetList[i] = new List<AsyncLoadResParam>();
            }

            m_Startmono = mono;
            m_Startmono.StartCoroutine(AsyncLoadCor());
        }

        public long CreateGuid()
        {
            return m_Guid++;
        }

        public void ClearCache()
        {
            List<ResourceItem> tempList = new List<ResourceItem>();
            foreach (var item in AssetDic.Values)
            {
                if (item.m_Clear)
                {
                    tempList.Add(item);
                }
            }

            foreach (var item in tempList)
            {
                DestoryResourceItem(item, true);
            }
            tempList.Clear();
        }

        //异步加载
        IEnumerator AsyncLoadCor()
        {
            List<AsyncCallBack> callBackList = null;
            long lastYieldTime = System.DateTime.Now.Ticks;
            while (true)
            {
                bool haveYield = false;
                for (int i = 0; i < (int) LoadResPriority.RES_NUM; i++)
                {
                    if (m_LoadingAssetList[(int) LoadResPriority.RES_HIGH].Count > 0)
                    {
                        i = (int) LoadResPriority.RES_HIGH;
                    } 
                    else if (m_LoadingAssetList[(int) LoadResPriority.RES_MIDDLE].Count > 0)
                    {
                        i = (int) LoadResPriority.RES_MIDDLE;
                    }

                    List<AsyncLoadResParam> loadingList = m_LoadingAssetList[i];
                    if (loadingList.Count <= 0)
                    {
                        continue;
                    }

                    AsyncLoadResParam loadingItem = loadingList[0];
                    loadingList.RemoveAt(0);
                    callBackList = loadingItem.m_CallBackList;
                    Object obj = null;
                    ResourceItem item = null;
#if UNITY_EDITOR
                    if (!m_LoadFromAssetBundle)
                    {
                        if (loadingItem.m_Sprite)
                        {
                            obj = LoadAssetByEditor<Sprite>(loadingItem.m_Path);
                        }
                        else
                        {
                            obj = LoadAssetByEditor<Object>(loadingItem.m_Path);
                        }
                        yield return new WaitForSeconds(0.5f);

                        item = AssetBundleManager.Instance.FindResourceItme(loadingItem.m_Crc);
                        if (item == null)
                        {
                            item = new ResourceItem();
                            item.m_Crc = loadingItem.m_Crc;
                        }
                    }
#endif
                    if (obj == null)
                    {
                        item = AssetBundleManager.Instance.LoadResourceAssetBundle(loadingItem.m_Crc);
                        if (item != null && item.m_AssetBundle != null)
                        {
                            AssetBundleRequest abRequest = null;
                            if (loadingItem.m_Sprite)
                            {
                                abRequest = item.m_AssetBundle.LoadAssetAsync<Sprite>(item.m_AssetName);
                            }
                            else
                            {
                                abRequest = item.m_AssetBundle.LoadAssetAsync(item.m_AssetName);
                            }

                            yield return abRequest;
                            if (abRequest.isDone)
                            {
                                obj = abRequest.asset;
                            }

                            lastYieldTime = System.DateTime.Now.Ticks;
                        }
                    }
                    CacheResource(loadingItem.m_Path,ref item, loadingItem.m_Crc,obj, callBackList.Count);
                    for (int j = 0; j < callBackList.Count; j++)
                    {
                        AsyncCallBack callBack = callBackList[j];
                        if (callBack != null && callBack.m_DealFinish != null && callBack.m_ResObj != null)
                        {
                            ResourceObj tempResourceObj = callBack.m_ResObj;
                            tempResourceObj.m_ResItem = item;
                            callBack.m_DealFinish(loadingItem.m_Path, tempResourceObj, tempResourceObj.m_Param1,
                                tempResourceObj.m_Param2, tempResourceObj.m_Param3);
                        }
                    }
                }
                
                
            }
        }
        

        public bool CancelLoad(ResourceObj res)
        {
            AsyncLoadResParam para = null;
            if (m_LoadingAssetDic.TryGetValue(res.m_Crc, out para) &&
                m_LoadingAssetList[(int) para.m_Priority].Contains(para))
            {
                for (int i = para.m_CallBackList.Count; i >= 0; i--)
                {
                    AsyncCallBack tempCallBack = para.m_CallBackList[i];
                    if (tempCallBack != null && res == tempCallBack.m_ResObj)
                    {
                        tempCallBack.Reset();
                        m_AsyncCallBackPool.Recycle(tempCallBack);
                        para.m_CallBackList.Remove(tempCallBack);
                    }

                }

                if (para.m_CallBackList.Count <= 0)
                {
                    para.Reset();
                    m_LoadingAssetList[(int) para.m_Priority].Remove(para);
                    m_AsyncLoadResParamPool.Recycle(para);
                    m_LoadingAssetDic.Remove(res.m_Crc);
                    return true;
                }
                
            }

            return false;
        }

        public int IncreaseResourceRef(uint crc = 0, int count = 1)
        {
            ResourceItem item = null;
            if (!AssetDic.TryGetValue(crc, out item) || item == null)
                return 0;
            item.RefCount += count;
            item.m_LastUseTime = Time.realtimeSinceStartup;
            return item.RefCount;
        }
        public int IncreaseResourceRef(ResourceObj resObj, int count = 1)
        {
            return resObj != null ? IncreaseResourceRef(resObj.m_Crc, count) : 0;
        }


        ResourceItem GetCacheResourceItem(uint crc, int addRefCount = 1)
        {
            ResourceItem item = null;
            if (AssetDic.TryGetValue(crc, out item))
            {
                if (item != null)
                {
                    item.RefCount += addRefCount;
                    item.m_LastUseTime = Time.realtimeSinceStartup;
                }
            }

            return item;
        }
        
        public void PreLoadRes(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            uint crc = Crc32.GetCrc32(path);
            ResourceItem item = GetCacheResourceItem(crc, 0);
            if (item != null)
            {
                return;
            }

            Object obj = null;
#if UNITY_EDITOR
            if (!m_LoadFromAssetBundle)
            {
                item = AssetBundleManager.Instance.FindResourceItme(crc);
                if (item != null && item.m_Obj != null)
                {
                    obj = item.m_Obj as Object;
                }
                else
                {
                    if (item == null)
                    {
                        item = new ResourceItem();
                        item.m_Crc = crc;
                    }
                    
                    obj = LoadAssetByEditor<Object>(path);
                }
                
            }
#endif
            if (obj == null)
            {
                item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
                if (item != null && item.m_AssetBundle != null)
                {
                    if (item.m_Obj != null)
                    {
                        obj = item.m_Obj;
                    }
                    else
                    {
                        obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
                    }
                }
            }

            CacheResource(path, ref item, crc, obj);
            item.m_Clear = false;
            ReleaseResource(obj, false);

        }

        public bool ReleaseResource(Object obj, bool destoryObj = false)
        {
            if (obj == null)
            {
                return false;
            }

            ResourceItem item = null;
            foreach (var  res in AssetDic.Values)
            {
                if (res.m_Guid == obj.GetInstanceID())
                {
                    item = res;
                }
            }

            if (item == null)
            {
                Debug.LogError("AssetDic里不存在该资源："+obj.name+" 可能释放了多次");
                return false;
            }

            item.RefCount--;
            DestoryResourceItem(item,destoryObj);
            return true;
        }
        
        public bool ReleaseResource(ResourceObj resObj, bool destoryObj = false)
        {
            if (resObj == null)
                return false;

            ResourceItem item = null;
            if (!AssetDic.TryGetValue(resObj.m_Crc, out item) || null == item)
            {
                Debug.LogError("AssetDic里面不存在该资源："+resObj.m_CloneObj.name+"可能释放了多次");
            }
            GameObject.Destroy(resObj.m_CloneObj);
            item.RefCount--;
            
            DestoryResourceItem(item,destoryObj);
            return true;
        }

        public T LoadResource<T>(string path) where T : UnityEngine.Object
        {
            if (String.IsNullOrEmpty(path))
            {
                return null;
            }

            uint crc = Crc32.GetCrc32(path);
            ResourceItem item = GetCacheResourceItem(crc);
            if (item != null)
            {
                return item.m_Obj as T;
            }

            T obj = null;
#if UNITY_EDITOR
            if (!m_LoadFromAssetBundle)
            {
                item = AssetBundleManager.Instance.FindResourceItme(crc);
                if (item != null && item.m_AssetBundle != null)
                {
                    if (item.m_Obj != null)
                    {
                        //已经加载了的AB包如果已经Instantiate了，就直接赋值
                        obj = (T)item.m_Obj;
                    }
                    else
                    {
                        //如果AB包已经被载入，但是还未Instantiate
                        obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
                    }
                }
                else
                {
                    if (item == null)
                    {
                        item = new ResourceItem();
                        item.m_Crc = crc;
                    }

                    obj = LoadAssetByEditor<T>(path);
                }
            }
#endif
            if (obj == null)
            {
                item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
                if (item != null && item.m_AssetBundle != null)
                {
                    if (item.m_Obj != null)
                    {
                        obj = item.m_Obj as T;
                    }
                    else
                    {
                        obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
                    }
                }
            }
            CacheResource(path,ref item,crc,obj);
            return obj;
        }
        
        //同步加载资源
        public ResourceObj LoadResource(string path, ResourceObj resObj)
        {
            if (resObj == null)
            {
                return null;
            }

            uint crc = resObj.m_Crc == 0 ? Crc32.GetCrc32(path) : resObj.m_Crc;
            ResourceItem item = GetCacheResourceItem(crc);

            if (item != null)
            {
                resObj.m_ResItem = item;
                return resObj;
            }

            Object obj = null;
            #if UNITY_EDITOR
            if (!m_LoadFromAssetBundle)
            {
                item = AssetBundleManager.Instance.FindResourceItme(crc);
                if (item != null && item.m_Obj != null)
                {
                    obj = item.m_Obj as Object;
                }
                else
                {
                    if (item == null)
                    {
                        item = new ResourceItem();
                        item.m_Crc = crc;
                    }

                    obj = LoadAssetByEditor<Object>(path);
                }
            }
            #endif
            if (obj == null)
            {
                item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
                if (item != null && item.m_AssetBundle != null)
                {
                    if (item.m_Obj != null)
                    {
                        obj = item.m_Obj as Object;
                    }
                    else
                    {
                        obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
                    }
                }
            }

            CacheResource(path, ref item, crc, obj);
            resObj.m_ResItem = item;
            item.m_Clear = resObj.m_bClear;

            return resObj;
        }


        //缓存加载资源
        void CacheResource(string path, ref ResourceItem item, uint crc, Object obj, int addrefcount = 1)
        {
            WashOut();
            if (item == null)
            {
                Debug.LogError("Resource item is null, path: " + path);
            }

            if (obj == null)
            {
                Debug.LogError("ResourceLoad Fail: "+path);
            }

            item.m_Obj = obj;
            item.m_Guid = obj.GetInstanceID();
            item.m_LastUseTime = Time.realtimeSinceStartup;
            item.RefCount += addrefcount;
            ResourceItem oldItem = null;
            if (AssetDic.TryGetValue(item.m_Crc, out oldItem))
            {
                AssetDic[item.m_Crc] = item;
            }
            else
            {
                AssetDic.Add(item.m_Crc, item);
            }
        }

        protected void WashOut()
        {
            while (m_NoRefrenceAssetMapList.Size() >= MAX_CACHE_COUNT)
            {
                for (int i = 0; i < MAX_CACHE_COUNT / 2; i++)
                {
                    ResourceItem item = m_NoRefrenceAssetMapList.Back();
                    DestoryResourceItem(item, true);
                }
            }
        }

        protected void DestoryResourceItem(ResourceItem item, bool destoryCache = false)
        {
            if (item == null || item.RefCount > 0)
            {
                return;
            }

            if (!destoryCache)
            {
                m_NoRefrenceAssetMapList.InsertToHead(item);
            }

            if (!AssetDic.Remove(item.m_Crc))
            {
                return;
            }
            m_NoRefrenceAssetMapList.Remove(item);
            //释放assetbundle引用
            AssetBundleManager.Instance.ReleaseAsset(item);
            //清空资源对应的对象池
            ObjectManager.Instance.ClearPoolObject(item.m_Crc);

            if (item.m_Obj != null)
            {
                item.m_Obj = null;
#if UNITY_EDITOR
                Resources.UnloadUnusedAssets();
#endif
            }
            
        }
        
#if UNITY_EDITOR
        protected T LoadAssetByEditor<T>(string path) where T : UnityEngine.Object
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        }
#endif

        public void AsyncLoadResource(string path, OnAsyncObjFinish dealFinish, LoadResPriority priority,
            bool isSprite = false, object param1 = null, object param2 = null, object param3 = null, uint crc = 0)
        {
            if (crc == 0)
            {
                crc = Crc32.GetCrc32(path);
            }

            ResourceItem item = GetCacheResourceItem(crc);
            if (item != null)
            {
                if (dealFinish != null)
                {
                    dealFinish(path, item.m_Obj, param1, param2, param3);
                }

                return;
            }

            AsyncLoadResParam para = null;
            if (!m_LoadingAssetDic.TryGetValue(crc, out para) || para == null)
            {
                para = m_AsyncLoadResParamPool.Spawn(true);
                para.m_Crc = crc;
                para.m_Path = path;
                para.m_Sprite = isSprite;
                m_LoadingAssetDic.Add(crc,para);
                m_LoadingAssetList[(int) priority].Add(para);
            }
            
            

        }
        
        
    }
    
    
    
    
}