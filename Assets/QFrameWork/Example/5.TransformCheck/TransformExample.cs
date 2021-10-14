using UnityEditor;
using UnityEngine;

namespace QFrameWork
{
    public partial class TransformUtil
    {
#if UNITY_EDITOR
        [MenuItem("QFrameWork/Example/4.Transform赋值优化")]
#endif
        private static void MenuSetLocalPosX()
        {
            var trans = new GameObject("transform").transform;
            SetLocalPosX(trans, 5.0f);
            SetLocalPosY(trans, 5.0f);
            SetLocalPosZ(trans, 5.0f);
        }
#if UNITY_EDITOR
        [MenuItem("QFrameWork/Example/5.Transform归一化")]
#endif
        private static void MenuIdentity()
        {
            var trans = new GameObject("transform").transform;
            TransformUtil.Identity(trans);
        }
#if UNITY_EDITOR
        [MenuItem("QFrameWork/Example/6.AddChild")]
#endif
        private static void MenuAddChild()
        {
            var transP = new GameObject("Parent").transform;
            var transC = new GameObject("Child").transform;
            TransformUtil.AddChild(transP,transC);
            GameObjectUtil.Hide(transC);
        }       

    }
}