using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MyRealFrame
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
        //是否放到场景节点下面
        public bool m_SetSceneParent = false;
        //实例化资源加载完成回调
        public OnAsyncObjectFinish m_DealObjFinish = null;
        //异步参数
        public object m_Param1, m_Param2, m_Param3 = null;
        //离线数据
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
            m_DealObjFinish = null;
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
        //资源加载完成后的回调，一般框架内定
        public OnAsyncResourceObjFinish MDealResourceObjFinish = null;

        public ResourceObj m_ResObj = null;

        //资源加载后的回调（一般不用于实例化）
        public OnAsyncObjectFinish MDealObjectFinish = null;

        public object m_Param1 = null, m_Param2 = null, m_Param3 = null;

        public void Reset()
        {
            MDealObjectFinish = null;
            MDealResourceObjFinish = null;
            m_Param1 = null;
            m_Param2 = null;
            m_Param3 = null;
        }
    }
  
    public delegate void OnAsyncObjectFinish(string path, Object obj, object param1 = null, object param2 = null,
        object param3 = null);
    
  
    public delegate void OnAsyncResourceObjFinish
    (string path, ResourceObj resouceObj, object param1 = null, object param2 = null,
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
        //最长连续加载资源的时间，单位微秒
        private const long MAX_LOAD_RES_TIME = 20000;
        private const int MAX_CACHE_COUNT = 500;

        public void Init(MonoBehaviour mono)
        {
            //加载配置文件
            AssetBundleManager.Instance.LoadAssetBundleConfig();
            //创建加载队列
            for (int i = 0; i < (int) LoadResPriority.RES_NUM; i++)
            {
                m_LoadingAssetList[i] = new List<AsyncLoadResParam>();
            }
            //开启协程
            m_Startmono = mono;
            m_Startmono.StartCoroutine(AsyncLoadCor());
        }

        public long CreateGuid()
        {
            return m_Guid++;
        }
        
        //一般用切换场景的时候使用，搭配ObjectManager.ClearCache
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
                    //如果在编辑器里吗开启非AB模式，则通过LoadAssetByEditor加载资源
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
                        //查找已经缓存的Item，没有就创建
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
                        //通过crc来加载ab，如果已经缓存了的就直接加载，否则就加载AB与其依赖
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
                        if (callBack != null && callBack.MDealResourceObjFinish != null && callBack.m_ResObj != null)
                        {
                            ResourceObj tempResourceObj = callBack.m_ResObj;
                            tempResourceObj.m_ResItem = item;
                            callBack.MDealResourceObjFinish(loadingItem.m_Path, tempResourceObj, tempResourceObj.m_Param1,
                                tempResourceObj.m_Param2, tempResourceObj.m_Param3);
                            callBack.MDealResourceObjFinish = null;
                            tempResourceObj = null;
                        }

                        if (callBack != null && callBack.MDealObjectFinish != null)
                        {
                            callBack.MDealObjectFinish(loadingItem.m_Path, obj, callBack.m_Param1, callBack.m_Param2,
                                callBack.m_Param3);
                            callBack.MDealObjectFinish = null;
                        }
                        callBack.Reset();
                        m_AsyncCallBackPool.Recycle(callBack);
                    }
                    if (System.DateTime.Now.Ticks - lastYieldTime > MAX_LOAD_RES_TIME)
                    {
                        yield return null;
                        lastYieldTime = System.DateTime.Now.Ticks;
                        haveYield = true;
                    }
                }
                if (!haveYield || System.DateTime.Now.Ticks - lastYieldTime > MAX_LOAD_RES_TIME)
                {
                    lastYieldTime = System.DateTime.Now.Ticks;
                    yield return null;
                }
            }
        }
        
        public int DecreaseResourceRef(ResourceObj resObj, int count = 1)
        {
            return resObj != null ? DecreaseResourceRef(resObj.m_Crc, count) : 0;
        }
        
        public int DecreaseResourceRef(uint crc, int count = 1)
        {
            ResourceItem item = null;
            if (!AssetDic.TryGetValue(crc, out item) || item == null)
                return 0;

            item.RefCount -= count;

            return item.RefCount;
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
        
        //预加载资源（同步加载)
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
                    break;
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
        
        //释放资源
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

        public ResourceItem GetResourceItem(string path)
        {
            uint crc = Crc32.GetCrc32(path);
            ResourceItem item = GetCacheResourceItem(crc);
            if (item == null)
            {
                Debug.Log("the resource does not loaded");
            }

            return item;
        }
        //同步加载，会阻塞
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
                return;
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
        /// <summary>
        /// 异步加载资源（仅仅是不需要实例化的资源，例如音频，图片等等）
        /// </summary>
        public void AsyncLoadResource(string path, OnAsyncObjectFinish dealFinish, LoadResPriority priority,
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
            
            //往回调列表里面加回调
            AsyncCallBack callBack = m_AsyncCallBackPool.Spawn(true);
            callBack.MDealObjectFinish = dealFinish;
            callBack.m_Param1 = param1;
            callBack.m_Param2 = param2;
            callBack.m_Param3 = param3;
            para.m_CallBackList.Add(callBack);
        }

        /// <summary>
        /// 针对ObjectManager的异步加载接口
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resObj"></param>
        /// <param name="dealfinish"></param>
        /// <param name="priority"></param>
        public void AsyncLoadResource(string path, ResourceObj resObj, OnAsyncResourceObjFinish dealfinish,
            LoadResPriority priority)
        {
            ResourceItem item = GetCacheResourceItem(resObj.m_Crc);
            if (item != null)
            {
                resObj.m_ResItem = item;
                if (dealfinish != null)
                {
                    dealfinish(path, resObj);
                }
                return;
            }

            AsyncLoadResParam para = null;
            if (!m_LoadingAssetDic.TryGetValue(resObj.m_Crc, out para) || para == null)
            {
                para = m_AsyncLoadResParamPool.Spawn(true);
                para.m_Crc = resObj.m_Crc;
                para.m_Path = path;
                para.m_Priority = priority;
                m_LoadingAssetDic.Add(resObj.m_Crc,para);
                m_LoadingAssetList[(int)priority].Add(para);
            }

            AsyncCallBack callback = m_AsyncCallBackPool.Spawn(true);
            callback.MDealResourceObjFinish = dealfinish;
            callback.m_ResObj = resObj;
            para.m_CallBackList.Add(callback);
        }
    }
    
    //双向链表结构节点
    public class DoubleLinkedListNode<T> where T : class, new()
    {
        //前一个节点
        public DoubleLinkedListNode<T> prev = null;
        //后一个节点
        public DoubleLinkedListNode<T> next = null;
        //当前节点
        public T t = null;
    }

    //双向链表结构
    public class DoubleLinedList<T> where T : class, new()
    {
        //表头
        public DoubleLinkedListNode<T> Head = null;
        //表尾
        public DoubleLinkedListNode<T> Tail = null;
        //双向链表结构类对象池
        protected ClassObjectPool<DoubleLinkedListNode<T>> m_DoubleLinkNodePool = ObjectManager.Instance.GetOrCreateClassPool<DoubleLinkedListNode<T>>(500);
        //个数
        protected int m_Count = 0;
        public int Count
        {
            get { return m_Count; }
        }

        /// <summary>
        /// 添加一个节点到头部
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public DoubleLinkedListNode<T> AddToHeader(T t)
        {
            DoubleLinkedListNode<T> pList = m_DoubleLinkNodePool.Spawn(true);
            pList.next = null;
            pList.prev = null;
            pList.t = t;
            return AddToHeader(pList);
        }

        /// <summary>
        /// 添加一个节点到头部
        /// </summary>
        /// <param name="pNode"></param>
        /// <returns></returns>
        public DoubleLinkedListNode<T> AddToHeader(DoubleLinkedListNode<T> pNode)
        {
            if (pNode == null)
                return null;

            pNode.prev = null;
            if (Head == null)
            {
                Head = Tail = pNode;
            }
            else
            {
                pNode.next = Head;
                Head.prev = pNode;
                Head = pNode;
            }
            m_Count++;
            return Head;
        }

        /// <summary>
        /// 添加节点到尾部
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public DoubleLinkedListNode<T> AddToTail(T t)
        {
            DoubleLinkedListNode<T> pList = m_DoubleLinkNodePool.Spawn(true);
            pList.next = null;
            pList.prev = null;
            pList.t = t;
            return AddToTail(pList);
        }

        /// <summary>
        /// 添加节点到尾部
        /// </summary>
        /// <param name="pNode"></param>
        /// <returns></returns>
        public DoubleLinkedListNode<T> AddToTail(DoubleLinkedListNode<T> pNode)
        {
            if (pNode == null)
                return null;

            pNode.next = null;
            if (Tail == null)
            {
                Head = Tail = pNode;
            }
            else
            {
                pNode.prev = Tail;
                Tail.next = pNode;
                Tail = pNode;
            }
            m_Count++;
            return Tail;
        }

        /// <summary>
        /// 移除某个节点
        /// </summary>
        /// <param name="pNode"></param>
        public void RemoveNode(DoubleLinkedListNode<T> pNode)
        {
            if (pNode == null)
                return;

            if (pNode == Head)
                Head = pNode.next;

            if (pNode == Tail)
                Tail = pNode.prev;

            if (pNode.prev != null)
                pNode.prev.next = pNode.next;

            if (pNode.next != null)
                pNode.next.prev = pNode.prev;

            pNode.next = pNode.prev = null;
            pNode.t = null;
            m_DoubleLinkNodePool.Recycle(pNode);
            m_Count--;
        }

        /// <summary>
        /// 把某个节点移动到头部
        /// </summary>
        /// <param name="pNode"></param>
        public void MoveToHead(DoubleLinkedListNode<T> pNode)
        {
            if (pNode == null || pNode == Head)
                return;

            if (pNode.prev == null && pNode.next == null)
                return;

            if (pNode == Tail)
                Tail = pNode.prev;

            if (pNode.prev != null)
                pNode.prev.next = pNode.next;

            if (pNode.next != null)
                pNode.next.prev = pNode.prev;

            pNode.prev = null;
            pNode.next = Head;
            Head.prev = pNode;
            Head = pNode;
            if (Tail == null)
            {
                Tail = Head;
            }
        }
    }

    public class CMapList<T> where T : class, new()
    {
        DoubleLinedList<T> m_DLink = new DoubleLinedList<T>();
        Dictionary<T, DoubleLinkedListNode<T>> m_FindMap = new Dictionary<T, DoubleLinkedListNode<T>>();

        ~CMapList()
        {
            Clear();
        }

        /// <summary>
        /// 情况列表
        /// </summary>
        public void Clear()
        {
            while (m_DLink.Tail != null)
            {
                Remove(m_DLink.Tail.t);
            }
        }

        /// <summary>
        /// 插入一个节点到表头
        /// </summary>
        /// <param name="t"></param>
        public void InsertToHead(T t)
        {
            DoubleLinkedListNode<T> node = null;
            if (m_FindMap.TryGetValue(t, out node) && node != null)
            {
                m_DLink.AddToHeader(node);
                return;
            }

            m_DLink.AddToHeader(t);
            m_FindMap.Add(t, m_DLink.Head);
        }

        /// <summary>
        /// 从表尾弹出一个结点
        /// </summary>
        public void Pop()
        {
            if (m_DLink.Tail != null)
            {
                Remove(m_DLink.Tail.t);
            }
        }

        /// <summary>
        /// 删除某个节点
        /// </summary>
        /// <param name="t"></param>
        public void Remove(T t)
        {
            DoubleLinkedListNode<T> node = null;
            if (!m_FindMap.TryGetValue(t, out node) || node == null)
            {
                return;
            }

            m_DLink.RemoveNode(node);
            m_FindMap.Remove(t);
        }

        /// <summary>
        /// 获取到尾部节点
        /// </summary>
        /// <returns></returns>
        public T Back()
        {
            return m_DLink.Tail == null ? null : m_DLink.Tail.t;
        }

        /// <summary>
        /// 返回节点个数
        /// </summary>
        /// <returns></returns>
        public int Size()
        {
            return m_FindMap.Count;
        }

        /// <summary>
        /// 查找是否存在该节点
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool Find(T t)
        {
            DoubleLinkedListNode<T> node = null;
            if (!m_FindMap.TryGetValue(t, out node) || node == null)
                return false;

            return true;
        }

        /// <summary>
        /// 刷新某个节点，把节点移动到头部
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool Reflesh(T t)
        {
            DoubleLinkedListNode<T> node = null;
            if (!m_FindMap.TryGetValue(t, out node) || node == null)
                return false;

            m_DLink.MoveToHead(node);
            return true;
        }
    }
}