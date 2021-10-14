using System;
using UnityEditor;

namespace QFrameWork.Editor
{
    public partial class Export
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