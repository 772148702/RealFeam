using UnityEditor;
using UnityEngine;

namespace QFrameWork
{
    public class ResolutionCheck
    {
        #if UNITY_EDITOR
                [MenuItem("QFrameWork/9.屏幕宽高比判断",false ,5)]
                private static void MenuAspectJudge()
                {
                    Debug.Log(IsPadScreen()?"是Pad分辨率":"不是Pad分辨率");
                    Debug.Log(IsPhoneResolution()?"是Iphone分辨率":"不是Iphone分辨率");
                    Debug.Log(IsPhoneXRResolution()?"是IphoneXR分辨率":"不是IphoneXR分辨率");
                }
        #endif
        
                public static float GetAspectRatio()
                {
                    var isLandscape = Screen.width > Screen.height;
                   // Debug.Log(isLandscape ? "横屏" : "竖屏");
                    float aspect;
                    if (isLandscape)
                    {
                        aspect = (float) Screen.width / Screen.height;
                    }
                    else
                    {
                        aspect = (float) Screen.height / Screen.width;
                    }
        
                    return aspect;
                }
        
                public static bool IsPadScreen()
                {
                    var aspect = GetAspectRatio();
                    var isPad = aspect > (4.0f / 3 - 0.05) && aspect < (4.0f / 3 + 0.05f);
                    return isPad;
                }
                
                //判断是否是手机分辨率
                public static bool IsPhoneResolution()
                {
                    var aspect = GetAspectRatio();
        
                    return aspect > 3.0f / 2 - 0.05 && aspect < 3.0f / 2 + 0.05;
                }
                //判断是否是XR手机分辨率
                public static bool IsPhoneXRResolution()
                {
                    var aspect = GetAspectRatio();
        
                    return aspect > 2436.0f / 1125 - 0.05 && aspect < 2436.0f / 1125 + 0.05;
                }
    }
}