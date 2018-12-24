using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.IO;

public class AssetboundleRead : EditorWindow
{
    private static List<WWW> loaderList = new List<WWW>();

    [MenuItem("[Tools]/AssetboundleRead")]
    public static void OpenAssetboundleReadWindow()
    {
        var assetboundleReadWindows = EditorWindow.GetWindow<AssetboundleRead>();
        assetboundleReadWindows.Init();
    }

    public void Init()
    {
       
    }

    public void LoadSelectedAssetBundle()
    {
        foreach (UnityEngine.Object tmp in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets))
        {
            string filePath = AssetDatabase.GetAssetPath(tmp);
            if (Path.GetExtension(filePath) == ".unity3d")
            {
                LoadGameObject(GetPathURL().Replace("Assets", "") + filePath);
            }
        }
    }

    //不同平台下StreamingAssets的路径是不同的，这里需要注意一下。
    public string GetPathURL()
    {
        return "file://" + Application.dataPath; 
    }

    //读取一个资源
    private void LoadGameObject(string path)
    {
        loaderList.Add(new WWW(path));
    }

    private void Update()
    {
        List<int> removeList = new List<int>();

        for (int i = 0; i < loaderList.Count; ++i)
        {
            if (loaderList[i].isDone)
            {
                //加载到游戏中
                if (loaderList[i].assetBundle != null)
                {
                    var objs = loaderList[i].assetBundle.LoadAllAssets();
                    loaderList[i].assetBundle.Unload(false);

                    if (objs != null && objs.Length > 0)
                    {
                        string txt = JsonUtility.ToJson(objs[0]);

                        string assetFullPath = "Assets/AssetBoundleRead/" + objs[0].GetType().Name + ".asset";
                        object objClass = ScriptableObject.CreateInstance(objs[0].GetType());
                        JsonUtility.FromJsonOverwrite(txt, objClass);
                        AssetDatabase.CreateAsset(objClass as UnityEngine.Object, assetFullPath);
                    }

                    
                }

                removeList.Add(i);
            }
        }

        for (int i = 0, count = removeList.Count; i < count; ++i)
        {
            loaderList.RemoveAt(removeList[i]);
            count--;
            i--;
        }
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Read AssetBoundle To AssetBoundleRead Dir"))
            {
                ToolCommon.ClearDirectory("Assets/AssetBoundleRead/");
                ToolCommon.CreatePath("Assets/AssetBoundleRead/");
                LoadSelectedAssetBundle();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}