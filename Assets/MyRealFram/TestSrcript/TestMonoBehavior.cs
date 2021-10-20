using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MyRealFram
{
    public class TestMonoBehavior : MonoBehaviour
    {
        [SerializeField]
        private Object temp;

        private ResourceItem _resourceItem;
        private void Start()
        {      
   
            MyRealFram.ResourceManager.Instance.Init(this);
            MyRealFram.OnAsyncObjectFinish cb = (string a, Object b, object c, object d, object e) =>
            {
                  //GameObject.Instantiate(b,gameObject.transform);
                  temp = b;
                  (temp as GameObject).transform.SetParent(this.transform);
                 Debug.Log("load successful");
            };
            
 
            // MyRealFram.ResourceManager.Instance.AsyncLoadResource("Assets/GameData/Prefabs/Attack.prefab",
            //     cb,LoadResPriority.RES_HIGH);
            MyRealFram.ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
            MyRealFram.ObjectManager.Instance.InstantiateObjAsync("Assets/GameData/Prefabs/Attack.prefab",cb,LoadResPriority.RES_HIGH);
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
            MyRealFram.ObjectManager.Instance.ReleaseObject(temp as GameObject,0);
            //MyRealFram.ResourceManager.Instance.ReleaseResource(temp);
        }
    }
}