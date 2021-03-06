using System;
using UnityEngine;

namespace QFrameWork
{
    public class AudioManager:MonoSingleton<AudioManager>
    {
    
        private AudioSource mMusicSource = null;
        private AudioListener mAudioListener;
        
        private void CheckAudioListener()
        {
            if (!mAudioListener)
            {
                mAudioListener = gameObject.AddComponent<AudioListener>();
            }
        }
        public void PlaySound(string soundName)
        {
            CheckAudioListener();
            var   aduioSource = gameObject.AddComponent<AudioSource>();
            var coinSound = MyRealFrame.ResourceManager.Instance.LoadResource<AudioClip>(soundName);
            aduioSource.clip = coinSound;
            aduioSource.Play();
        }
        
        public void PlayMusic(string musicName,bool loop)
        {
            CheckAudioListener();
            if (!mMusicSource)
            {
                mMusicSource = gameObject.AddComponent<AudioSource>();
            }
            var coinSound = MyRealFrame.ResourceManager.Instance.LoadResource<AudioClip>(musicName);
            mMusicSource.clip = coinSound;
            mMusicSource.loop = loop;
            mMusicSource.Play();
        }
        
        public void StopMusic()
        {
            mMusicSource.Stop();
        }
        public void PauseMusic()
        {
            mMusicSource.Pause();
        }
        public void ResumeMusic()
        {
            mMusicSource.UnPause();
        }
        
        public void MusicOff()
        {
            mMusicSource.Pause();
            mMusicSource.mute = true;
        }
        public void SoundOff()
        {
            var audioSources = GetComponents<AudioSource>();
            foreach (var audioSource in audioSources)
            {
                if (audioSource != mMusicSource)
                {
                    audioSource.Pause();
                    audioSource.mute = true;
                }
            }
        }
        
        public void MusicOn()
        {
            mMusicSource.UnPause();
            mMusicSource.mute = false;
        }
        public void SoundOn()
        {
            var audioSources = GetComponents<AudioSource>();
            foreach (var audioSource in audioSources)
            {
                if (audioSource != mMusicSource)
                {
                    audioSource.UnPause();
                    audioSource.mute = false;
                }
            }
        }

    }
}