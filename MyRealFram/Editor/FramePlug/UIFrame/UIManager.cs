using System;
using System.Collections.Generic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MyRealFram
{
    public enum UIMsgId
    {
        None = 0,
    }
    
    public class UIManager
    {
        public RectTransform m_UiRoot;
        public RectTransform m_WndRoot;
        private Camera m_UICamera;
        private EventSystem m_EventSystem;
        private float m_CanvasRate = 0;
        private string m_UIPrefabPath = "Assets/GameData/Prefabs/UGUI/Panel";
        //注册的字典
        private Dictionary<string, System.Type> m_RegisterDic = new Dictionary<string, Type>();
        private Dictionary<string, Window> m_WindowDic = new Dictionary<string, Window>();
        private List<MyRealFram.Window> m_WindowList = new List<MyRealFram.Window>();

        public void Init(RectTransform uiRoot, RectTransform wndRoot, Camera uiCamera, EventSystem eventSystem)
        {
            m_UiRoot = uiRoot;
            m_WndRoot = wndRoot;
            m_UICamera = uiCamera;
            m_EventSystem = eventSystem;
            m_CanvasRate = Screen.height / (m_UICamera.orthographicSize * 2);
        }

        public void SetUIPrefab(string path)
        {
            m_UIPrefabPath = path;
        }

        public void ShowOrHideUI(bool show)
        {
            if (m_UiRoot != null)
            {
                m_UiRoot.gameObject.SetActive(true);
            }
        }

        public void SetNormalSelectObj(GameObject obj)
        {
            if (m_EventSystem == null)
            {
                m_EventSystem = EventSystem.current;
            }

            m_EventSystem.firstSelectedGameObject = obj;
        }

        public void OnUpdate()
        {
            for (int i = 0; i < m_WindowList.Count; i++)
            {
                if (m_WindowList[i] != null)
                {
                    m_WindowList[i].OnUpdate();
                }
            }
        }

        public void Register<T>(string name) where T : Window
        {
            m_RegisterDic[name] = typeof(T);
        }

        public bool SendMessageToWnd(string name, UIMsgId msgId = 0, params object[] paralist)
        {
            Window wnd = FindWndByName<Window>(name);
            if (wnd != null)
            {
                return wnd.OnMessage(msgId, paralist);
            }

            return false;
        }

        public T FindWndByName<T>(string name) where T : Window
        {
            Window wnd = null;
            if (m_WindowDic.TryGetValue(name, out wnd))
            {
                return (T) wnd;
            }

            return null;
        }

        public Window PopUpWnd(string wndName, bool bTop = true, params object[] paralist)
        {
            Window wnd = FindWndByName<Window>(wndName);
            if (wnd == null)
            {
                System.Type tp = null;
                if (m_RegisterDic.TryGetValue(wndName, out tp))
                {
                    wnd =System.Activator.CreateInstance(tp) as Window;
                }
                else
                {
                    Debug.LogError("找不到窗口对应的脚本，窗口名是：" + wndName);
                    return null;
                }
                GameObject wndObj = ObjectManager.Instance.InstantiateObject(m_UIPrefabPath + wndName, false, false);
                if (wndObj == null)
                {
                    Debug.Log("创建窗口Prefab失败：" + wndName);
                    return null;
                }
                if (!m_WindowDic.ContainsKey(wndName))
                {
                    m_WindowList.Add(wnd);
                    m_WindowDic.Add(wndName, wnd);
                }
                wnd.GameObject = wndObj;
                wnd.Transform = wndObj.transform;
                wnd.Name = wndName;
                wnd.Awake(paralist);
                wndObj.transform.SetParent(m_WndRoot, false);
                if (bTop)
                {
                    wndObj.transform.SetAsLastSibling();
                }

                wnd.OnShow(paralist);
            }
            else
            {
                ShowWnd(wndName, bTop, paralist);
            }
            return wnd;
        }
        
        public void ShowWnd(string name, bool bTop = true, params object[] paralist)
        {
            Window wnd = FindWndByName<Window>(name);
            ShowWnd(wnd, bTop, paralist);
        }
        public void ShowWnd(Window wnd, bool bTop = true, params object[] paralist)
        {
            if (wnd != null)
            {
                if (wnd.GameObject != null && !wnd.GameObject.activeSelf) wnd.GameObject.SetActive(true);
                if (bTop) wnd.Transform.SetAsLastSibling();
                wnd.OnShow(paralist);
            }
        }
    }
}