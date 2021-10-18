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
        //变量名称
        public string Name { get; set; }
        //变量类型
        public string Type { get; set; }
        //变量中的excel中的列
        public string Col { get; set; }
        //变量的默认值
        public string DefaultValue { get; set; }
        //变量是list的话，外联部分列
        public string Foregin { get; set; }
        //分隔符号
        public string SplitStr { get; set; }
        //如果自己是List，对应的list类名
        public string ListName { get; set; }
        //如果自己是list，对应的sheet名字
        public string ListSheetName { get; set; }
    }

    public class SheetData
    {
        public List<string> AllName = new List<string>();
        public List<string> AllType = new List<string>();
        public List<RowData> AllData = new List<RowData>();
    }

    public class RowData
    {
        public string ParnetValue = "";
        public Dictionary<string, string> RowDataDic = new Dictionary<string, string>();
    }

    public enum TestEnum
    {
        None = 0,
        VAR1=1,
        Test2=2
    }

    public class TestInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsA { get; set; }
        public bool Heigh { get; set; }
        public TestEnum TestType { get; set; }
        public List<string> AllStrList { get; set; }
        public List<TestInfoTwo> AllTestInfoList { get; set; }
    }

    public class TestInfoTwo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
