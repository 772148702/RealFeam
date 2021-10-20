using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class MMonoSingleleton<T> : MonoBehaviour where T: MMonoSingleleton<T>
{
        public static T Instance
        {
            get
            {
                return instance;
            }
        }
        protected static T instance;

        public void Awake()
        {
            if(instance==null)
            {
                // var ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                // var ctor = Array.Find(ctors, c => c.GetParameters().Length == 0);
                // if (ctor == null)
                // {
                //     throw new Exception("No-public ctor not find");
                // }
                // instance = ctor.Invoke(null) as T;
                instance = (T)this;
            } else
            {
                UnityEngine.Debug.Log("Singleleton error");
            }
        }
}

