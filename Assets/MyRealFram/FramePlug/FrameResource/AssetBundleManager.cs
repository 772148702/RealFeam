using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace MyRealFram
{
    public class AssetBundleManager : Singleton<AssetBundleManager>
    {
        protected string m_ABConfigABName = "assetbundleconfig";

        //资源关系依赖配表，可以根据crc来找到对应资源块
        protected Dictionary<uint, ResourceItem> m_ResouceItemDic = new Dictionary<uint, ResourceItem>();

        //存储已经加载的AB包，key为crc
        protected Dictionary<uint, AssetBundleItem> m_AssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();

        //AssetBundleItme对象池
        protected ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool =
          ObjectManager.Instance.GetOrCreateClassPool<AssetBundleItem>(500);
        
        protected string ABLoadPath
        {
            get { return Application.streamingAssetsPath + "/"; }
        }

        public bool LoadAssetBundleConfig()
        {
#if UNITY_EDITOR
            if (!ResourceManager.Instance.m_LoadFromAssetBundle)
            {
                return false;
            }
#endif
            m_ResouceItemDic.Clear();
            string configPath = ABLoadPath + m_ABConfigABName;
            AssetBundle configAB = AssetBundle.LoadFromFile(configPath);
            TextAsset textAsset = configAB.LoadAsset<TextAsset>(m_ABConfigABName);
            if (textAsset == null)
            {
                Debug.LogError("AssetBundleConfig is not exist");
                return false;
            }

            MemoryStream memoryStream = new MemoryStream(textAsset.bytes);
            BinaryFormatter bf = new BinaryFormatter();
            MyRealFram.AssetBundleConfig config = (MyRealFram.AssetBundleConfig) bf.Deserialize(memoryStream);
            memoryStream.Close();
            for (int i = 0; i < config.AbBases.Count; i++)
            {
                ABBase abBase = config.AbBases[i];
                MyRealFram.ResourceItem item = new MyRealFram.ResourceItem();
                item.m_Crc = abBase.Crc;
                item.m_AssetName = abBase.AssetName;
                item.m_ABName = abBase.ABName;
                item.m_DependAssetBundle = abBase.ABDependence;
                if (m_ResouceItemDic.ContainsKey(item.m_Crc))
                {
                    Debug.LogError("重复Crc 资源名字:" + item.m_AssetName + " ab包名" + item.m_ABName);
                }
                else
                {
                    m_ResouceItemDic.Add(item.m_Crc, item);
                }
            }

            return true;
        }
        
        //通过crc来加载ab，如果已经缓存了的就直接加载，否则就加载AB与其依赖
        public ResourceItem LoadResourceAssetBundle(uint crc)
        {
            ResourceItem item = null;
            if (!m_ResouceItemDic.TryGetValue(crc, out item) || item == null)
            {
                Debug.LogError(string.Format("LoadResourceAssetBundle error: can not find crc {0} in AssetBundleConfig", crc.ToString()));
                return item;
            }

            if (item.m_AssetBundle != null)
            {
                return item;
            }

            item.m_AssetBundle = LoadAssetBundle(item.m_ABName);
            if (item.m_DependAssetBundle != null)
            {
                for (int i = 0; i < item.m_DependAssetBundle.Count; i++)
                {
                    LoadAssetBundle(item.m_DependAssetBundle[i]);
                }
            }
            return item;
        }
        
        //通过AB包名字的crc的来存储缓存
        private AssetBundle LoadAssetBundle(string name)
        {
            AssetBundleItem item = null;
            uint crc = Crc32.GetCrc32(name);
            if (!m_AssetBundleItemDic.TryGetValue(crc, out item))
            {
                AssetBundle assetBundle = null;
                string fullPath = ABLoadPath + name;
                assetBundle = AssetBundle.LoadFromFile(fullPath);
                if (assetBundle == null)
                {
                    Debug.LogError("Load AssetBundle Error: "+fullPath);
                }

                item = m_AssetBundleItemPool.Spawn(true);
                item.assetBundle = assetBundle;
                item.RefCount++;
                m_AssetBundleItemDic.Add(crc,item);
            }
            else
            {
                item.RefCount++;
            }

            return item.assetBundle;
        }

        public void ReleaseAsset(ResourceItem item)
        {

            if (item == null) return;
            if (item.m_DependAssetBundle != null && item.m_DependAssetBundle.Count > 0)
            {
                for (int i = 0; i < item.m_DependAssetBundle.Count; i++)
                {
                    UnLoadAssetBundle(item.m_DependAssetBundle[i]);
                }
            }
            UnLoadAssetBundle(item.m_ABName);
        }

        private void UnLoadAssetBundle(string name)
        {
            AssetBundleItem item = null;
            uint crc = Crc32.GetCrc32(name);
            if (m_AssetBundleItemDic.TryGetValue(crc, out item) && item != null)
            {
                item.RefCount--;
                if(item.RefCount<=0&&item.assetBundle!=null)
                {
                    item.assetBundle.Unload(true);
                    item.Reset();
                    m_AssetBundleItemPool.Recycle(item);
                    m_AssetBundleItemDic.Remove(crc);
                }
            }
        }

        //查找已经缓存的Item，没有就创建
        public ResourceItem FindResourceItme(uint crc)
        {
            ResourceItem item = null;
            m_ResouceItemDic.TryGetValue(crc, out item);
            return item;
        }
    }

    public class AssetBundleItem
    {
        public AssetBundle assetBundle = null;
        public int RefCount;

        public void Reset()
        {
            assetBundle = null;
            RefCount = 0;
        }
    }
   
    //当个资源
    public class ResourceItem
    {
        public uint m_Crc = 0;
        public string m_AssetName = String.Empty;
        public string m_ABName = String.Empty;
        public List<string> m_DependAssetBundle = null;
        public AssetBundle m_AssetBundle = null;
        public UnityEngine.Object m_Obj = null;
        public int m_Guid = 0;
        public float m_LastUseTime = 0.0f;
        protected int m_RefCount = 0;
        public bool m_Clear = true;

        public int RefCount
        {
            get { return m_RefCount; }
            set
            {
                m_RefCount = value;
                if (m_RefCount < 0)
                {
                    Debug.LogError("refcount < 0" + m_RefCount + " ," + (m_Obj != null ? m_Obj.name : "name is null"));
                    
                }
            }
        }

    }
}