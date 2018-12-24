using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConfigPathSet))]
public class ConfigPathSetInspector : Editor 
{
    public override bool UseDefaultMargins()
    {
        return false;
    }

    public override void OnInspectorGUI()
    {
        ConfigPathSet config = ConfigPathSet.GetConfig();

        GUILayout.Space(20);

        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.BeginHorizontal();
            {
                string newDataPath = EditorGUILayout.TextField("导入txt目录", config.strTxtPath);
                if (GUILayout.Button("...", GUILayout.MaxWidth(24)))
                {
                    GUIUtility.keyboardControl = 0;
                    newDataPath = EditorUtility.OpenFolderPanel("导入txt目录", config.strTxtPath, "");
                }

                if (newDataPath != "" && newDataPath != config.strTxtPath)
                {
                    config.strTxtPath = newDataPath;
                    EditorUtility.SetDirty(config);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                string newDataPath = EditorGUILayout.TextField("导入cs目录", config.strCsPath);
                if (GUILayout.Button("...", GUILayout.MaxWidth(24)))
                {
                    GUIUtility.keyboardControl = 0;
                    newDataPath = EditorUtility.OpenFolderPanel("导入cs目录", config.strCsPath, "");
                }

                if (newDataPath != "" && newDataPath != config.strCsPath)
                {
                    config.strCsPath = newDataPath;
                    EditorUtility.SetDirty(config);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                string newJsonPath = EditorGUILayout.TextField("生成assetboundle路径", config.strBoundlePath);
                if (GUILayout.Button("...", GUILayout.MaxWidth(24)))
                {
                    GUIUtility.keyboardControl = 0;
                    newJsonPath = EditorUtility.OpenFolderPanel("生成assetboundle路径", config.strBoundlePath, "");
                }

                if (newJsonPath != "" && newJsonPath != config.strBoundlePath)
                {
                    config.strBoundlePath = newJsonPath;
                    EditorUtility.SetDirty(config);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(20);
    }
}
