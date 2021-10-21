using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MyRealFrame
{
    public class TestMonoBehavior : MonoBehaviour
    {
        [SerializeField]
        private Object temp;

        private ResourceItem _resourceItem;
        private void Start()
        {      
   
            ResourceManager.Instance.Init(this);
            OnAsyncObjectFinish cb = (string a, Object b, object c, object d, object e) =>
            {
                  //GameObject.Instantiate(b,gameObject.transform);
                  temp = b;
                  (temp as GameObject).transform.SetParent(this.transform);
                 Debug.Log("load successful");
            };
            
 
            // MyRealFram.ResourceManager.Instance.AsyncLoadResource("Assets/GameData/Prefabs/Attack.prefab",
            //     cb,LoadResPriority.RES_HIGH);
            ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
            ObjectManager.Instance.InstantiateObjAsync("Assets/GameData/Prefabs/Attack.prefab",cb,LoadResPriority.RES_HIGH);
            ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UGUI/Test1.png",
                (string a, Object b, object c, object d, object e) =>
                {
                    Debug.Log((b as Sprite).rect);
                }, LoadResPriority.RES_SLOW,true);
            StartCoroutine(WaitForSecond());
        }


        public IEnumerator WaitForSecond()
        {
            yield return new WaitForSeconds(3.0f);
            ObjectManager.Instance.ReleaseObject(temp as GameObject,0);
            //MyRealFram.ResourceManager.Instance.ReleaseResource(temp);
        }
    }
}