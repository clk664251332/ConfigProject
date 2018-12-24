using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class ConfigPathSet : ScriptableObject {

    private static string CONFIG_PATH = "Assets/ToolScripts/Editor/Config/ConfigPathSet.asset";

    private static string PROJECT_TMP_PATH          //策划数据临时存放目录
    = "/MyResources/outputtxt";
    private static string PROJECT_CS_PATH           //自动生成CS目录
    = "/Scripts/StaticData/ScriptObject";

    public string projectTxtPath;  //工程用来存放配置的临时目录
    public string projectCSPath; //工程来放置CS的目录

    public string strTxtPath;         //导入txt目录
    public string strCsPath;          //导入cs目录
    public string strBoundlePath;     //导出boundle目录

#if UNITY_EDITOR
    public static ConfigPathSet GetConfig()
    {
        ConfigPathSet config = AssetDatabase.LoadAssetAtPath<ConfigPathSet>(CONFIG_PATH);

        if (config == null)
        {
            config = ScriptableObject.CreateInstance<ConfigPathSet>();

            if (!Directory.Exists(Path.GetDirectoryName(CONFIG_PATH)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CONFIG_PATH));
            }

            AssetDatabase.CreateAsset(config, CONFIG_PATH);
        }

        config.projectTxtPath = Application.dataPath + PROJECT_TMP_PATH;
        config.projectCSPath = Application.dataPath + PROJECT_CS_PATH;

        return config;
    }
#endif
}
