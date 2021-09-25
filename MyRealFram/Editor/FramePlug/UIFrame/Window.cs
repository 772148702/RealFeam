using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyRealFram
{
    public class Window
    {
        //引用的GameObject
        public GameObject GameObject { get; set; }
        //引用的Transfrom
        public Transform Transform { get; set; }
        //物体本身的Name
        public string Name { get; set; }
        //所有的Button
        protected List<Button> m_AllButton = new List<Button>();
        //所有的Toggle
        protected List<Toggle> m_AllToggle = new List<Toggle>();

        public virtual bool OnMessage(UIMsgId msgId, params object[] paralist)
        {
            return true;
        }

        public virtual void Awake(params object[] paralist)
        {
        }

        public virtual void OnShow(params object[] paralist)
        {
            
        }

        public virtual void OnUpdate() { }
        public virtual void OnDisable()
        {
            
        }

        public virtual void OnClose()
        {
                
        }

        public bool ChangeImageSprite(string path, Image image, bool setNativeSize = false)
        {
            if (image == null) return false;
            Sprite sp = ResourceManager.Instance.LoadResource<Sprite>(path);
            if (sp != null)
            {
                if (image.sprite != null)
                {
                    image.sprite = null;
                }

                image.sprite = sp;
                if (setNativeSize)
                {
                    image.SetNativeSize();
                }

                return true;
            }

            return false;
        }

        public void ChangeImageSpriteAsync(string path, Image image, bool setNativeSize = false)
        {
            if (image == null) return;
            
            ResourceManager.Instance.AsyncLoadResource(path,OnLoadSpriteFinish,LoadResPriority.RES_MIDDLE,image,setNativeSize);
        }

        void OnLoadSpriteFinish(string path, Object obj, object param1 = null, object param2 = null,
            object param3 = null)
        {
            if (obj != null)
            {
                Sprite sp = obj as Sprite;
                Image image = param1 as Image;
                bool setNativeSize = (bool) param2;
                if (image.sprite != null)
                {
                    image.sprite = null;
                }

                image.sprite = sp;
                if (setNativeSize)
                {
                    image.SetNativeSize();
                }
            }
        }

        public void RemoveAllButtonListeners()
        {
            foreach (var bnt in m_AllButton)
            {
                bnt.onClick.RemoveAllListeners();
            }
        }
        
        public void RemoveAllToggleListener()
        {
            foreach (Toggle toggle in m_AllToggle)
            {
                toggle.onValueChanged.RemoveAllListeners();
            }
        }
        void BtnPlaySound()
        {

        }
        public void AddButtonClickListener(Button btn, UnityEngine.Events.UnityAction action) 
        {
            if (btn != null)
            {
                if (!m_AllButton.Contains(btn))
                {
                    m_AllButton.Add(btn);
                }
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(action);
                btn.onClick.AddListener(BtnPlaySound);
            }
        }
        public void AddToggleClickListener(Toggle toggle , UnityEngine.Events.UnityAction<bool> action)
        {
            if (toggle != null)
            {
                if (!m_AllToggle.Contains(toggle))
                {
                    m_AllToggle.Add(toggle);
                }
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener(action);
                toggle.onValueChanged.AddListener(TogglePlaySound);
            }
        }
        void TogglePlaySound(bool isOn)
        {

        }
    }
}