

using System;
using UnityEngine;

namespace QFrameWork
{
    public class TestGuiManager : MonoBehaviourSimplify
    {
        private void Awake()
        {
            MyRealFrame.ResourceManager.Instance.Init(this);
            MyRealFrame.ObjectManager.Instance.Init();
        }

        private void Start()
        {
       
            GUIManager.LoadPanelAsync("Assets/GameData/UGUI/Button.prefab",GUIManager.UILayer.Common);
       
            this.Delay(3.0f, () =>
            {    
                GUIManager.SetResolution(1280, 720, 0);
                GUIManager.UnLoadPanel("Assets/GameData/UGUI/Button.prefab");
            });
        }

        protected override void OnBeforeDestory()
        {
           // throw new System.NotImplementedException();
        }
    }
}