using UnityEditor;
using UnityEngine;

namespace QFrameWork
{
    public partial class ResolutionCheck
    {
#if UNITY_EDITOR
        [MenuItem("QFrameWork/Example/3.屏幕宽高比判断",false ,5)]
        private static void MenuAspectJudge()
        {
            Debug.Log(IsPadScreen()?"是Pad分辨率":"不是Pad分辨率");
            Debug.Log(IsPhoneResolution()?"是Iphone分辨率":"不是Iphone分辨率");
            Debug.Log(IsPhoneXRResolution()?"是IphoneXR分辨率":"不是IphoneXR分辨率");
        }
#endif
    }
}