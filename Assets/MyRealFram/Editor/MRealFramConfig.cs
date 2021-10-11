using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Assets", menuName = "MRealFramConfig")]
public class MRealFramConfig:ScriptableObject
{
    public string xmlPath;
    public string abPath;
    public string scriptPath;
    public string binaryPath;
}

[CustomEditor(typeof(MRealFramConfig))]
public class MRealFramConfigInspector:Editor
{
    public SerializedProperty xmlPath ;
    public SerializedProperty abPath;
    public SerializedProperty scriptPath;
    public SerializedProperty binaryPath;

    private void OnEnable()
    {
        xmlPath = serializedObject.FindProperty("xmlPath");
        abPath = serializedObject.FindProperty("abPath");
        scriptPath = serializedObject.FindProperty("scriptPath");
        binaryPath = serializedObject.FindProperty("binaryPath");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(abPath, new GUIContent("ab包二进制路径"));
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(xmlPath, new GUIContent("xml配置路径"));
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(scriptPath, new GUIContent("配置脚本路径"));
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(binaryPath, new GUIContent("配置二进制路径"));
        GUILayout.Space(5);
        serializedObject.ApplyModifiedProperties();
    }
}

public class MRealConfig
{
    private const string MRealFramPath = "Assets/MyRealFram/Editor/ABConfig.asset";

    public static MRealFramConfig  GetRealFram()
    {
        MRealFramConfig realConfig = AssetDatabase.LoadAssetAtPath<MRealFramConfig>(MRealFramPath);
        return realConfig;
    }
} 


