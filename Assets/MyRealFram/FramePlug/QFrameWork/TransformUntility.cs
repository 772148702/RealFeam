using UnityEditor;
using UnityEngine;

namespace QFrameWork
{
    public class TransformUntility
    {
        
#if UNITY_EDITOR
        [MenuItem("QFrameWork/10.Transform赋值优化")]
#endif
        private static void MenuSetLocalPosX()
        {
            var trans = new GameObject("transform").transform;
            SetLocalPosX(trans,5.0f);
            SetLocalPosY(trans,5.0f);
            SetLocalPosZ(trans,5.0f);
        }
        
        public static void SetLocalPosX(Transform transform, float x)
        {
            var localPos = transform.localPosition;
            localPos.x = x;
            transform.localPosition = localPos;
        }
        public static void SetLocalPosY(Transform transform, float y)
        {
            var localPos = transform.localPosition;
            localPos.y = y;
            transform.localPosition = localPos;
        }
        public static void SetLocalPosZ(Transform transform, float z)
        {
            var localPos = transform.localPosition;
            localPos.z = z;
            transform.localPosition = localPos;
        }

        public static void SetLocalPosXY(Transform transform, float x, float y)
        {
            var localPos = transform.localPosition;
            localPos.x = x;
            localPos.y = y;
            transform.localPosition = localPos;
        }
        public static void SetLocalPosXZ(Transform transform, float x, float z)
        {
            var localPos = transform.localPosition;
            localPos.x = x;
            localPos.z = z;
            transform.localPosition = localPos;
        }
        
        public static void SetLocalPosYZ(Transform transform, float y, float z)
        {
            var localPos = transform.localPosition;
            localPos.y = y;
            localPos.z = z;
            transform.localPosition = localPos;
        }

#if UNITY_EDITOR
        [MenuItem("QFrameWork/11.Transform归一化")]
#endif
        private static void MenuIdentity()
        {
            var trans = new GameObject("transform").transform;
            Identity(trans);
        }
        
        public static void Identity(Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }


        public static void Show(GameObject gameObject)
        {
            gameObject.SetActive(true);
        }

        public static void Hide(GameObject gameObject)
        {
            gameObject.SetActive(false);
        }
    }
}