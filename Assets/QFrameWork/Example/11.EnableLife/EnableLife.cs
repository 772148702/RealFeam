using System;
using UnityEngine;

namespace QFrameWork
{
    public class EnableLife:MonoBehaviourSimplify
    {
        private void Awake()
        {
            this.gameObject.SetActive(false);
            // this.Delay(3.0f,(() =>
            // {
            //     Debug.Log("3.0f Passed");
            //     this.gameObject.SetActive(true);
            // }));
        }

        private void OnEnable()
        {
            Debug.Log("On Enable");
        }

        protected override void OnBeforeDestory()
        {
            //throw new System.NotImplementedException();
        }
    }
}