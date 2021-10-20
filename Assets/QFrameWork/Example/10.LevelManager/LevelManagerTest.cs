
using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace QFrameWork
{
    public class LevelManagerTest:MonoBehaviourSimplify
    {
        
#if UNITY_EDITOR
        [UnityEditor.MenuItem("QFrameWork/Example/14.LevelManager", false, 
            13)]
        private static void MenuClicked()
        {
            UnityEditor.EditorApplication.isPlaying = true;
            new GameObject("LevelExample").AddComponent<LevelManagerTest>();
        }
#endif
        private void Start()
        {
            DontDestroyOnLoad(this);
            LevelManager.Init(new List<string>
            {
                "Home",
                "Level",
            });
            LevelManager.LoadCurrent();
            this.Delay(10, () =>
            {
                LevelManager.LoadNext();
                Debug.Log("Load Next");
            });
        }

        protected override void OnBeforeDestory()
        {
            
        }
    }
}