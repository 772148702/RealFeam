using UnityEditor;

namespace QFrameWork
{
    public class CustomShortCut
    {
    #if UNITY_EDITOR
            [MenuItem("QFrameWork/1.0导出package %e",false,-10)]
    #endif
            private static void MenuReuseKey()
            {
                ExportUtil.ExportPackage();
            }
                
    }
}