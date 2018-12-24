using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.IO;

public class CreateConfigAssetboundleSelected
{
    [MenuItem("[Build Windows]/打包配置文件(txt选中导入)")]
    public static void ExecuteWindow()
    {
        Exec(BuildTarget.StandaloneWindows);
    }

    public static void Exec(BuildTarget target)
    {
        Dictionary<string, string> dicPath = new Dictionary<string, string>();
        foreach (UnityEngine.Object o in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets))
        {
            string filePath = AssetDatabase.GetAssetPath(o);
            if(Path.GetExtension(filePath) == ".txt")
            {
                 string fileName = Path.GetFileNameWithoutExtension(filePath);
                 string assetFilePath = ToolCommon.GetAssetPath(filePath);
                
                if(!dicPath.ContainsKey(fileName))
                {
                    dicPath.Add(fileName, assetFilePath);
                }
            }
        }

        ConfigPathSet configPathSet = ConfigPathSet.GetConfig();
        string boundlePath = configPathSet.strBoundlePath;

        if(!CreateConfigAssetboundle.CheckPath(ref boundlePath))
        {
            return;
        }

        CreateConfigAssetboundle.InitTempLogs();
        CreateConfigAssetboundle.ExecutePath(boundlePath, target, dicPath);
    }
}