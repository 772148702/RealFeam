uisng UnityEngine;


class MClassObjectPool<T> where T: class, new()
{
    public bool spanwIfPoolEmpty = true;
    public int needRecycleCount = 0;
    public int MAX_SIZE = 50;
    Stack<T> _stack = new Stack<T>();

    public T Spawn()
    {
        if(_stack.Count>0)
        {
          T tempT= _stack.Pop();
          needRecycleCount++;
          if(tempT==null)
          {
      
             return T tempT = new T();
          } else 
          {
              return tempT;
          }
        } else 
        {
            if(spanwIfPoolEmpty)
            {
                needRecycleCount++;
                return T tempT = new T();
            } else {
                return null;
            }
        }
        return null;
   }
   
   public bool Recycle(T obj)
   {
       if(obj==null){
           return false;
       }
       needRecycleCount--;
       if(_stack.Count>=MAX_SIZE&&MAX_SIZE>0) {
           obj = null;
           return false;
       }

      _stack.Push(obj);
      obj = null;
   
      return true;
   }


}