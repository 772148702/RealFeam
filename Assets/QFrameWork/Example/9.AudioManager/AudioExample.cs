using System;
using QFrameWork;
using UnityEngine;
namespace QFramework
{
    public class AudioExample : MonoBehaviourSimplify
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("QFrameWork/Example/13.AudioManager", false, 
            13)]
        private static void MenuClicked()
        {
            UnityEditor.EditorApplication.isPlaying = true;
            new GameObject("AudioExample").AddComponent<AudioExample>();
        }
#endif
        private void Awake()
        {
            MyRealFram.ResourceManager.Instance.Init(this);
            MyRealFram.ObjectManager.Instance.Init();
        }

        private void Start()
        {
            AudioManager.Instance.PlaySound("Assets/GameData/Sounds/menusound.mp3");
            AudioManager.Instance.PlaySound("Assets/GameData/Sounds/menusound.mp3");
            this.Delay(1.0f, () =>
            {
                AudioManager.Instance.PlayMusic("Assets/GameData/Sounds/senlin.mp3", true);
            });
            this.Delay(3.0f, () =>
            {
                AudioManager.Instance.PauseMusic();
            });
            this.Delay(5.0f, () =>
            {
                AudioManager.Instance.ResumeMusic();
            });
            this.Delay(7.0f, () =>
            {
                AudioManager.Instance.StopMusic();
            });
            
            this.Delay(9.0f, () => { AudioManager.Instance.PlayMusic("Assets/GameData/Sounds/senlin.mp3", true); });
            
            this. Delay(11.0f, () => { AudioManager.Instance.MusicOff(); });
            this.Delay(13.0f, () => { AudioManager.Instance.MusicOn(); });
        }

        protected override void OnBeforeDestory()
        {
            
        }
    }
}