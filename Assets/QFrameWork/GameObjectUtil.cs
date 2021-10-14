using UnityEngine;

namespace QFrameWork
{
    public class GameObjectUtil
    {
        public static void Show(GameObject gameObject)
        {
            gameObject.SetActive(true);
        }
        
        public static void Show(Transform transform)
        {
            transform.gameObject.SetActive(true);
        }


        public static void Hide(GameObject gameObject)
        {
            gameObject.SetActive(false);
        }
        
        public static void Hide(Transform transform)
        {
            transform.gameObject.SetActive(false);
        }
    }
}