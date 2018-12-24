using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ConfigAssetBundleWriter {

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
        AssetDatabase.Refresh();

        if (BuildPipeline.BuildAssetBundles(boundlePath, buildList.ToArray(), BuildAssetBundleOptions.None, target))
        {
            for (int i = 0; i < buildList.Count; i++)
            {
                //@lcm 暂时去掉路径，否则bat那边会输出乱码
                Debug.Log("Create " /*+ boundlePath + "/"*/ + buildList[i].assetBundleName);
            }
        }
        else
        {
            Debug.LogError("Create AssetBoundle Failed");
        }

        //foreach (var buildInfo in buildList)
        //{
        //    if (buildInfo.assetNames.Length > 0)
        //        AssetDatabase.DeleteAsset(buildInfo.assetNames[0]);
        //}

        ToolCommon.ClearBuild(boundlePath);
        AssetDatabase.Refresh();
    }

    private static string GetExtensionByBuildTarget(BuildTarget target)
    {
        if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
        {
            return ".unity3d";
        }
        else if (target == BuildTarget.Android)
        {
            return ".unity3d";
        }
        else if (target == BuildTarget.iOS)
        {
            return ".unity3d";
        }

        return "";
    }

}
