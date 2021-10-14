using System;
using System.Diagnostics;
using UnityEngine;

namespace QFrameWork
{
    public enum EnvironmentMode
    {
        Developing,
        Test,
        Production
    }
    
    public abstract class MainManager:MonoBehaviour
    {
        public EnvironmentMode Mode;

        private void Start()
        {
            switch (Mode)
            {
                case EnvironmentMode.Developing:
                    LaunchInDevelopingMode();
                    break;
                case EnvironmentMode.Test:
                    LaunchInProductionMode();
                    break;
                case EnvironmentMode.Production:
                    LaunchInTestMode();
                    break;
                
            }
        }
        protected abstract void LaunchInDevelopingMode();
        protected abstract void LaunchInProductionMode();
        protected abstract void LaunchInTestMode();
    }
}