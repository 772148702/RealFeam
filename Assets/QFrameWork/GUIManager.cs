using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QFrameWork
{
    public class GUIManager:MonoBehaviour
    {
        public enum UILayer
        {
            Bg,
            Common,
            Top
        }

        private static GameObject mPrivateUiRoot;

        public static GameObject UIRoot
        {
            get
            {
                if (mPrivateUiRoot == null)
                {
                   
                    MyRealFrame.ObjectManager.Instance.InstantiateObjAsync("Assets/GameData/UGUI/UIRoot.prefab",(
                        (path, o, param1, param2, param3) =>
                        {
                            mPrivateUiRoot = o as GameObject;
                        } ));
                }
                return mPrivateUiRoot;
            }
        }

        private static Dictionary<string, GameObject> mPanelDic = new Dictionary<string, GameObject>();
        
        public static void SetResolution(float width,float height,float
            matchWidthOrHeight)
        {
            var canvasScaler = UIRoot.GetComponent<CanvasScaler>();
            canvasScaler.referenceResolution = new Vector2(width, height);
            canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
        }

        
        public static GameObject LoadPanel(string path,UILayer layer)
        {
            
           GameObject obj =   MyRealFrame.ResourceManager.Instance.LoadResource<GameObject>(path);
           var panel = GameObject.Instantiate(obj);
           switch (layer)
           {
               case UILayer.Bg:
                   panel.transform.SetParent(UIRoot.transform.Find("Bg"));
                   break;
               case UILayer.Common:
                   panel.transform.SetParent(UIRoot.transform.Find("Common"));
                   break;
               case UILayer.Top:
                   panel.transform.SetParent(UIRoot.transform.Find("Top"));
                   break;
           }
           
           var rectTrans = panel.transform as RectTransform;
           rectTrans.offsetMin = Vector2.zero;
           rectTrans.offsetMax = Vector2.zero;
           rectTrans.anchoredPosition3D = Vector2.zero;
           rectTrans.anchorMax = Vector2.one;
           rectTrans.anchorMin = Vector2.zero;
           mPanelDic.Add(path,panel);
           return panel;
        }
        
        
        public static void LoadPanelAsync(string resPath, UILayer layer,GameObject panel=null)
        {
            if (UIRoot == null)
            {
                Debug.Log("Start to Load UIRoot");
            }
            
            MyRealFrame.ObjectManager.Instance.InstantiateObjAsync(resPath,(
                (path, o, param1, param2, param3) =>
                {
                      panel = o as GameObject;
                    switch (layer)
                    {
                        case UILayer.Bg:
                            panel.transform.SetParent(UIRoot.transform.Find("Bg"));
                            break;
                        case UILayer.Common:
                            panel.transform.SetParent(UIRoot.transform.Find("Common"));
                            break;
                        case UILayer.Top:
                            panel.transform.SetParent(UIRoot.transform.Find("Top"));
                            break;
                    }
           
                    var rectTrans = panel.transform as RectTransform;
                    rectTrans.offsetMin = Vector2.zero;
                    rectTrans.offsetMax = Vector2.zero;
                    rectTrans.anchoredPosition3D = Vector2.zero;
                    rectTrans.anchorMax = Vector2.one;
                    rectTrans.anchorMin = Vector2.zero;
                    
                    mPanelDic.Add(path,panel);
                } ));
            
        }

        public static void UnLoadPanel(string panelName)
        {
            if (mPanelDic.ContainsKey(panelName))
            {
                Destroy(mPanelDic[panelName]);
            }
        }
        
        
    }
}