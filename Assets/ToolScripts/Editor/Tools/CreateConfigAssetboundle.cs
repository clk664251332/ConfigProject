using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using UnityEditor;
using System.IO;
using UnityEditor.Callbacks;


[Serializable]
public class IOTempConfigFileInfo
{
    public bool bExe = false;
    public string boundlePath = string.Empty;
    public BuildTarget buildTarget = BuildTarget.NoTarget;
    public List<string> diffFileName = new List<string>();
    public List<string> diffResourceName = new List<string>();
}

[Serializable]
public class IOTempConfigLogInfo
{
    public List<string> logList = new List<string>();
}

public class CreateConfigAssetboundle
{
    private static List<string> m_logList = new List<string>();

    [MenuItem("[Build Windows]/打包配置文件(ConfigPathSet导入)")]
    public static void ExecuteWindow()
    {
        ConfigPathSet configPathSet = ConfigPathSet.GetConfig();

        InitTempLogs();

        if(!Exec(BuildTarget.StandaloneWindows, configPathSet.strBoundlePath, configPathSet.strTxtPath, configPathSet.strCsPath))
        {
            SaveLogToFile();
        }
    }

    static bool Exec(BuildTarget target, string boundlePath, string srcTxtPath, string srcCsPath)
    {
        InitConfigFile();

        if (!CheckPath(ref boundlePath))
        {
            return false;
        }

        if (!CheckPath(ref srcTxtPath))
        {
            return false;
        }

        if (!CheckPath(ref srcCsPath))
        {
            return false;
        }

        Dictionary<string, string> dicDiffPath = new Dictionary<string, string>();

        GetDifffTxtPath(srcTxtPath, ref dicDiffPath);
        CopyToCsPath(srcCsPath);

        if (dicDiffPath.Count <= 0)
        {
            Log("Log: ", "import txt is not diff");
            Debug.LogError("导入数据失败， 不存在需要导入的txt文件");
            return false;
        }

        SaveConfigFile(dicDiffPath, boundlePath, target);


        Log("Log0: ", "0");
        AssetDatabase.Refresh();
        Log("Log1: ", "1");

        if (dicDiffPath.Count > 0)
        {
            OnCompileScripts();
        }

        Log("LogEnd: ", "LogEnd");
        SaveLogToFile();

        return true;
    }

    static void SaveConfigFile(Dictionary<string, string> dicDiffPath, string boundlePath, BuildTarget target)
    {
        IOTempConfigFileInfo configFileInfo = IOHelper.GetData<IOTempConfigFileInfo>(DEFINE.IO_HELPER_TEMP_CONFIG_FILE_INFO);
        if (configFileInfo == null)
        {
            Debug.LogError("configFileInfo == null");
        }

        configFileInfo.bExe = true;
        configFileInfo.buildTarget = target;
        configFileInfo.boundlePath = boundlePath;

        configFileInfo.diffFileName.Clear();
        configFileInfo.diffResourceName.Clear();
        foreach (var pathInfo in dicDiffPath)
        {
            configFileInfo.diffFileName.Add(pathInfo.Key);
            configFileInfo.diffResourceName.Add(pathInfo.Value);
        }

        IOHelper.SetData(DEFINE.IO_HELPER_TEMP_CONFIG_FILE_INFO, configFileInfo);
    }

    static void InitConfigFile()
    {
        ToolCommon.CreatePath(SingletonObject<GameConfig>.GetInst().ResourcePath);

        IOTempConfigFileInfo configFileInfo = new IOTempConfigFileInfo();
        IOHelper.SetData(DEFINE.IO_HELPER_TEMP_CONFIG_FILE_INFO, configFileInfo);
    }

    public static void InitTempLogs()
    {
        ToolCommon.CreatePath(SingletonObject<GameConfig>.GetInst().ResourcePath);

        //m_logList.Clear();
        IOTempConfigLogInfo configLogInfo = new IOTempConfigLogInfo();
        IOHelper.SetData(DEFINE.IO_HELPER_TEMP_CONFIG_LOG_INFO, configLogInfo);
    }

     //[DidReloadScripts]
     public static void OnCompileScripts()
     {
        LoadTempFileLog();

        IOTempConfigFileInfo configFileInfo = IOHelper.GetData<IOTempConfigFileInfo>(DEFINE.IO_HELPER_TEMP_CONFIG_FILE_INFO);
        if (configFileInfo == null)
        {
            SaveLogToFile();
            return;
        }

        if (!configFileInfo.bExe)
        {
            SaveLogToFile();
            return;
        }

        Dictionary<string, string> dicPath = new Dictionary<string, string>();
        if (configFileInfo.diffFileName.Count != configFileInfo.diffResourceName.Count)
        {
            Log("Error: ", "configFileInfo.diffFileName.Count != configFileInfo.diffResourceName.Count");
            Debug.LogError("configFileInfo.diffFileName.Count != configFileInfo.diffResourceName.Count");
            SaveLogToFile();
            return;
        }

        for (int i = 0, count = configFileInfo.diffFileName.Count; i < count; i++)
        {
            if (!dicPath.ContainsKey(configFileInfo.diffFileName[i]))
            {
                dicPath.Add(configFileInfo.diffFileName[i], configFileInfo.diffResourceName[i]);
            }
        }
        Debug.Log(configFileInfo.boundlePath);
        Debug.Log(configFileInfo.buildTarget);

        Log("Log2: ", "OnCompileScripts dicPath.Count = " + dicPath.Count);
        ExecutePath(configFileInfo.boundlePath, configFileInfo.buildTarget, dicPath);
        Log("Log3: ", "OnCompileScripts dicPath.Count = " + dicPath.Count);

        //if (dicPath.Count == 0)
        //{
        //    InitConfigFile();
        //    return;
        //}

        SaveLogToFile();
    }

     public static void ExecutePath(string boundlePath, BuildTarget target, Dictionary<string, string> dicPath)
     {
         List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();

         Type subType = typeof(ScriptableObject);

         Assembly assembly = Assembly.Load("Assembly-CSharp");
         Type[] types = assembly.GetTypes();
         foreach (var objType in types)
         {
             if (objType.IsSubclassOf(subType))
             {
                 if (!objType.IsDefined(typeof(ConfigNameAttribute), false))
                 {
                     continue;
                 }

                 if (!objType.IsDefined(typeof(SerializableAttribute), false))
                 {
                     continue;
                 }

                 var name = (Attribute.GetCustomAttribute(objType, typeof(ConfigNameAttribute)) as ConfigNameAttribute).Name;
                 if (!dicPath.ContainsKey(name))
                 {
                     continue;
                 }

                 string fileName = dicPath[name];
                 TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(fileName);
                 if (textAsset == null)
                 {
                     Log("Error: ", "Can not find fileName = " + fileName);
                     Debug.LogError("Can not find fileName = " + fileName);
                     continue;
                 }

                 object objClass = ScriptableObject.CreateInstance(objType);
                 JsonUtility.FromJsonOverwrite(textAsset.text, objClass);

                 string assetFullPath = "Assets/AssetTemp/" + objClass.GetType().Name + ".asset";
                 AssetDatabase.CreateAsset(objClass as UnityEngine.Object, assetFullPath);

                 AssetBundleBuild buildInfo = new AssetBundleBuild();
                 buildInfo.assetBundleName = objClass.GetType().Name + GetExtensionByBuildTarget(target);
                 buildInfo.assetNames = new string[] { assetFullPath };

                 buildList.Add(buildInfo);

                 dicPath.Remove(name);
             }
         }


         Log("buildList.Count = ",  buildList.Count.ToString());
         Log("Log4: ", "4");
         AssetDatabase.Refresh();
         Log("Log5: ", "5");

         if (BuildPipeline.BuildAssetBundles(boundlePath, buildList.ToArray(), BuildAssetBundleOptions.None, target))
         {
             for (int i = 0; i < buildList.Count; i++)
             {
                //@lcm 暂时去掉路径，否则bat那边会输出乱码
                 Log("Log: ", "Create "  + buildList[i].assetBundleName);
                 Debug.Log("Create " /*+ boundlePath + "/"*/ + buildList[i].assetBundleName);
             }
         }
         else
         {
             Log("Error: ", "Create AssetBoundle Failed");
             Debug.LogError("Create AssetBoundle Failed");
         }

         //foreach (var buildInfo in buildList)
         //{
         //    if (buildInfo.assetNames.Length > 0)
         //        AssetDatabase.DeleteAsset(buildInfo.assetNames[0]);
         //}

         ToolCommon.ClearBuild(boundlePath);

         Log("Log6: ", "6");
         AssetDatabase.Refresh();
         Log("Log7 ", "7");
     }

	public static void BatExecute()
    {
        InitTempLogs();

        string[] path = System.Environment.GetCommandLineArgs();

        int paramStart = 0;
        bool bParamStart = false;
        bool bParamEnd = false;
        bool bParamStartFirst = false;
        int paramIndex = 0; //外部传参索引
        string boundlePath = string.Empty;
        string txtPath = string.Empty;
        string csPath = string.Empty;
        string buildTarget = string.Empty;
        for (int i = 0; i < path.Length; i++)
        {
            if (path[i] == "-quit")
            {
                bParamEnd = true;
            }

            if (bParamStart && !bParamStartFirst)
            {
                bParamStartFirst = true;
                paramStart = i;
            }

            if (bParamStart && !bParamEnd)
            {
                paramIndex = i - paramStart;
                if (paramIndex == 0)
                {
                    boundlePath = path[i];
                }
                else if (paramIndex == 1)
                {
                    txtPath = path[i];
                }
                else if (paramIndex == 2)
                {
                    csPath = path[i];
                }
                else if (paramIndex == 3)
                {
                    buildTarget = path[i];
                }
            }

            if (path[i] == "CreateConfigAssetboundle.BatExecute")
            {
                bParamStart = true;
            }
        }

        BuildTarget target = GetTargetByString(buildTarget);
        if (target == BuildTarget.NoTarget)
        {
            SaveLogToFile();
            return;
        }

        if (!Exec(target, boundlePath, txtPath, csPath))
        {
            SaveLogToFile();
        }
    }

    public static bool CheckPath(ref string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Log("Error: ", "path = " + path + " is not empty or null");
            Debug.LogError("path = " + path + " is not empty or null");
            return false;
        }

        if (!(Directory.Exists(path)))
         {
             Log("Error: ", "path = " + path + "can not find");
             Debug.LogError("path = " + path +  "can not find");   
             return false;
         }

        path = path.Replace("\\", "/");

        return true;
    }

    static string GetFileNameByCsName(string csFileName)
    {
        Assembly assembly = Assembly.Load("Assembly-CSharp");
        Type[] types = assembly.GetTypes();
        foreach (var objType in types)
        {
            if (objType.Name == csFileName)
            {
                if (!objType.IsDefined(typeof(ConfigNameAttribute), false))
                {
                    continue;
                }

                var name = (Attribute.GetCustomAttribute(objType, typeof(ConfigNameAttribute)) as ConfigNameAttribute).Name;

                return name;
            }
        }

        return string.Empty;
    }
    
    //是否有不同的txt资源需要打包
    static bool GetDifffTxtPath(string srcPath, ref Dictionary<string, string> dicDiffPath)
    {
        List<string> diffList = new List<string>();

        ConfigPathSet configPathSet = ConfigPathSet.GetConfig();
        string destPath = configPathSet.projectTxtPath;
        if (!CheckPath(ref destPath))
        {
            return false;
        }

        ImprotStaticDataResult importResult = ImportStaticData.ExecConfigPathSet(diffList, srcPath, destPath);
        if (importResult != ImprotStaticDataResult.Success)
        {
            Log("Log: ", "import static Txt failed!\n error code : " + importResult.ToString());
            Debug.Log("导入静态数据Txt失败!\n 错误代码 : " + importResult.ToString());
            return false;
        }

        foreach (var path in diffList)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            string assetPath = ToolCommon.GetAssetPath(path);

            if (!dicDiffPath.ContainsKey(fileName))
            {
                dicDiffPath.Add(fileName, assetPath);
            }
        }

        return true;
    }

    //是否有不同的cs资源需要打包
    static bool CopyToCsPath(string srcPath)
    {
        List<string> diffList = new List<string>();

        ConfigPathSet configPathSet = ConfigPathSet.GetConfig();
        string destPath = configPathSet.projectCSPath;
        if (!CheckPath(ref destPath))
        {
            return false;
        }

        string projectTxtPath = configPathSet.projectTxtPath;
        if (!CheckPath(ref projectTxtPath))
        {
            return false;
        }

        ImprotStaticDataResult importResult = ImportStaticData.ExecConfigPathSet(diffList, srcPath, destPath);
        if (importResult != ImprotStaticDataResult.Success)
        {
            Log("Log: ", "import Cs failed!\n error code : " + importResult.ToString());
            Debug.Log("导入静态数据Cs失败!\n 错误代码 : " + importResult.ToString());
            return false;
        }

        return true;
    }

    static string GetPascalName(string name)
    {
        string newName = name[0].ToString().ToUpper() + name.Substring(1);
        int index = 0;
        while(true)
        {
            index = newName.IndexOf("_", index);
            if(index == -1)
            {
                break;
            }

            index++;
            if(index == newName.Length - 1)
            {
                break;
            }

            newName = newName.Substring(0, index - 1) + newName[index].ToString().ToUpper() + newName.Substring(index + 1);
        }

        return newName;
    }

    static string GetExtensionByBuildTarget(BuildTarget target)
    {
        if(target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
        {
            return ".unity3d";
        }
        else if(target == BuildTarget.Android)
        {
            return ".unity3d";
        }
        else if(target == BuildTarget.iOS)
        {
            return ".unity3d";
        }

        return "";
    }

    static BuildTarget GetTargetByString(string target)
    {
        if(target == "Window")
        {
            return BuildTarget.StandaloneWindows;
        }
        else if(target == "Android")
        {
            return BuildTarget.Android;
        }
        else if(target == "IOS")
        {
            return BuildTarget.iOS;
        }

        Log("Error: ", "target = " + target + " BuildTarget.NoTarget");
        Debug.LogError("target = " + target + " BuildTarget.NoTarget");

        return BuildTarget.NoTarget;
    }

    static string GetExtensionByString(string target)
    {
        if (target == "Window")
        {
            return "";
        }
        else if (target == "Android")
        {
            return ".android";
        }
        else if (target == "IOS")
        {
            return ".ios";
        }

        Log("Error: ", "target = " + target + " Extension.NoTarget");
        Debug.LogError("target = " + target + " Extension.NoTarget");

        return "";
    }

    static void Log(string msgTag, string msg)
    {
        string save = System.DateTime.Now.ToString("HH:mm:ss ffff") + "  " + msgTag + msg;
        //m_logList.Add(save);

        //Debug.Log(save);

        ToolCommon.CreatePath(SingletonObject<GameConfig>.GetInst().ResourcePath);

        IOTempConfigLogInfo logFile = IOHelper.GetData<IOTempConfigLogInfo>(DEFINE.IO_HELPER_TEMP_CONFIG_LOG_INFO);
        if (logFile != null)
        {
            logFile.logList.Add(save);
        }
        IOHelper.SetData(DEFINE.IO_HELPER_TEMP_CONFIG_LOG_INFO, logFile);

        Debug.Log(save);
    }

    static void SaveLogToTempFile()
    {
        ToolCommon.CreatePath(SingletonObject<GameConfig>.GetInst().ResourcePath);

        IOTempConfigLogInfo logFile = IOHelper.GetData<IOTempConfigLogInfo>(DEFINE.IO_HELPER_TEMP_CONFIG_LOG_INFO);
        if(logFile != null)
        {
            logFile.logList.AddRange(m_logList);
        }
        IOHelper.SetData(DEFINE.IO_HELPER_TEMP_CONFIG_LOG_INFO, logFile);
    }

    static void LoadTempFileLog()
    {
        m_logList.Clear();
    }

    static void SaveLogToFile()
    {
        ToolCommon.CreatePath(SingletonObject<GameConfig>.GetInst().ResourcePath);

        StreamWriter streamWriter = null;
        try
        {
            streamWriter = File.CreateText(SingletonObject<GameConfig>.GetInst().ResourcePath + "ConfigSystem.log");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e.ToString());
        }

        IOTempConfigLogInfo ioTempConfigLog = IOHelper.GetData<IOTempConfigLogInfo>(DEFINE.IO_HELPER_TEMP_CONFIG_LOG_INFO);
        if(ioTempConfigLog == null)
        {
            return;
        }

        List<string> logList = ioTempConfigLog.logList;
        if (streamWriter != null)
        {
            if (logList.Count != 0)
            {
                for (int i = 0; i < logList.Count; i++)
                    streamWriter.WriteLine(logList[i]);

                streamWriter.Flush();
            }
        }

        if (streamWriter != null)
        {
            streamWriter.Close();
            streamWriter = null;
        }
    }
}
