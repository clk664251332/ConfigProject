//从指定目录导入配置数据到工程中的指定目录
//配置文件：Assets/ToolScripts/Config/ImportStaticDataConfig.asset
//例: 指定策划配置好的数据目录：E:/ConquerZero/策划设计/Config/数据表格/UnicodeTxT
//    工程中指定存放配置数据的目录：E:\ConquerZero\Projects\ConquerZero\Client_PVP\Assets\MyResources\staticdata

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

//导入配置数据
#if UNITY_EDITOR

public enum ImprotStaticDataResult
{
    Success,
    NoInitCopyList,
    EmptyConfig,
    EmptyCopy,
}

class ImportStaticData
{
    public static ImprotStaticDataResult ExecConfigPathSet(List<string> copyList, string srcPath, string destPath)
    {
        ConfigPathSet config = ConfigPathSet.GetConfig();

        if (string.IsNullOrEmpty(destPath) || string.IsNullOrEmpty(srcPath))
        {
            Selection.activeObject = config;
            return ImprotStaticDataResult.EmptyConfig;
        }

        if (copyList == null)
            return ImprotStaticDataResult.NoInitCopyList;

        CopyFile(srcPath, destPath, copyList);

        AssetDatabase.Refresh();
        
        if(copyList.Count > 0)
            return ImprotStaticDataResult.Success;
        else
            return ImprotStaticDataResult.EmptyCopy;
    }

    //public static ImprotStaticDataResult Exec(string extension, BuildTarget target, List<string> copyList)
    //{
    //    BuildStaticDataConfig config = BuildStaticDataConfig.GetConfig();
        
    //    if (string.IsNullOrEmpty(config.projectConfigPath) || string.IsNullOrEmpty(config.importPath))
    //    {
    //        Selection.activeObject = config;
    //        return ImprotStaticDataResult.EmptyConfig;
    //    }

    //    if (copyList == null)
    //        return ImprotStaticDataResult.NoInitCopyList;

    //    CopyFile(config.importPath, config.projectConfigPath, copyList);

    //    AssetDatabase.Refresh();
        
    //    if(copyList.Count > 0)
    //        return ImprotStaticDataResult.Success;
    //    else
    //        return ImprotStaticDataResult.EmptyCopy;
    //}

    public static void CopyFile(string path, string outputPath, List<string> copyList)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);

        if (dirInfo.Exists)
        {
            foreach (var file in dirInfo.GetFiles())
            {
                if (Path.GetExtension(file.Name) == ".txt" || Path.GetExtension(file.Name) == ".cs")
                {
                    var outputFilePath = outputPath + "/" + file.Name;

                    copyList.Add(outputFilePath);

                    FileInfo targetfileInfo = new FileInfo(outputFilePath);

                    if (targetfileInfo.Exists)
                    {
                        string targetMD5 = ToolCommon.GetFileMD5(targetfileInfo.FullName);
                        string sourceMD5 = ToolCommon.GetFileMD5(file.FullName);

                        if (targetMD5 == sourceMD5)
                            continue;
                    }
                    
                    File.Copy(file.FullName, outputFilePath, true);
                    Debug.Log("导入静态数据：" + file.FullName);

                    //copyList.Add(outputFilePath);
                }
            }

            foreach (var directory in dirInfo.GetDirectories())
            {
                CopyFile(directory.FullName, outputPath, copyList);
            }
        }
    }
}
#endif