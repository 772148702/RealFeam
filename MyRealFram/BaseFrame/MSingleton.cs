using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MSingleton<T>  where T: new()
{
     public static T Instance
    {
        get
        {
            if(instance==null)
            {
                instance = new T();
            }
            return instance;
        }
        set
        {
            instance = value;
        }
    }
    private static T instance;   

}

