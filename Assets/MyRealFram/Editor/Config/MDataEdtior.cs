using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MDataEdtior
{
    public static string AbPath = MRealConfig.GetRealFram().abPath;
    public static string XmlPath = MRealConfig.GetRealFram().xmlPath;
    public static string BinaryPath = MRealConfig.GetRealFram().binaryPath;
    public static string ScriptPath = MRealConfig.GetRealFram().scriptPath;
    public static string ExcelPath = Application.dataPath + "../Data/MExcel/";
    public static string RegPath = Application.dataPath + "../Data/MReg";

    [MenuItem("MAssets/类转Excel")]
    public static void AssetClassToXml()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i=0;i<objs.Length;i++)
        {
            EditorUtility.DisplayProgressBar("文件转换成xml","正在扫描"+objs[i].name+"....",1.0f/objs.Length*i);
            //Process
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    
    [MenuItem("MAssets/XML转Binary")]
    public static void AssetsXmlToBinary()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i=0;i<objs.Length;i++)
        {
            EditorUtility.DisplayProgressBar("文件下的xml转成二进制","正在扫描"+objs[i].name+"....",1.0f/objs.Length*i);
            //Process
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    
    [MenuItem("MAssets/Xml转Excel")]
    public static void AssetsXmlToExcel()
    {
        UnityEngine.Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("文件下的xml转成Excel", "正在扫描" + objs[i].name + "... ...", 1.0f / objs.Length * i);
            //XmlToExcel(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    
    [MenuItem("MTools/Xml/Xml转成二进制")]
    public static void AllXmlToBinary()
    {
        string path = Application.dataPath.Replace("Assets", "") + XmlPath;
        string[] filesPath = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < filesPath.Length; i++)
        {
            EditorUtility.DisplayProgressBar("查找文件夹下面的Xml", "正在扫描" + filesPath[i] + "... ...", 1.0f / filesPath.Length * i);
            if (filesPath[i].EndsWith(".xml"))
            {
                string tempPath = filesPath[i].Substring(filesPath[i].LastIndexOf("/") + 1);
                tempPath = tempPath.Replace(".xml", "");
               // XmlToBinary(tempPath);
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    
    [MenuItem("MTools/Xml/Excel转Xml")]
    public static void AllExcelToXml()
    {
        string[] filePaths = Directory.GetFiles(RegPath, "*", SearchOption.AllDirectories);
        for (int i = 0; i < filePaths.Length; i++)
        {
            if (!filePaths[i].EndsWith(".xml"))
                continue;
            EditorUtility.DisplayProgressBar("查找文件夹下的类","正在扫描路径" + filePaths[i] + "... ...", 1.0f / filePaths.Length * i);
            string path = filePaths[i].Substring(filePaths[i].LastIndexOf("/") + 1);
           // ExcelToXml(path.Replace(".xml", ""));
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    //将类转换成excel
    public static void ClassToXml(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        try
        {
            Type type = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type tempType = asm.GetType(name);
                if (tempType != null)
                {
                    type = tempType;
                    break;
                }
            }

            if (type != null)
            {
                var temp = Activator.CreateInstance(type);
                if (temp is ExcelBase)
                {
                    (temp as ExcelBase).Construction();
                }

                string xmlPath = XmlPath + name + ".xml";
                BinarySerializeOpt.Xmlserialize(xmlPath, temp);
                Debug.Log(name + "类转xml成功，xml路径为:" + xmlPath);
            }
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public class SheetClass
    {
        public VarClass ParentVar { get; set; }
        public int Depth { get; set; }
        public string Name { get; set; }
        public string SheetName { get; set; }
        public string MainKey { get; set; }
        public string SplitStr { get; set; }
        public List<VarClass> VarList = new List<VarClass>();
    }

    public class VarClass
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
