using System;
using System.Collections.Generic;
using System.Linq;
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
                instance = (T)this;
            } else
            {
            UnityEngine.Debug.Log("Singleleton error");
            }
        
        }
}

