using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class CsvMainLoader : Editor{

    private static List<string> m_lstCSV = new List<string>();
    private static Dictionary<string, string> m_dictOutputTxt = new Dictionary<string, string>();
    private static string m_strTxtPath = ProjectPath + "/Assets/MyResources/outputtxt";
    //private static string m_strConfigAssetBundlePath = ProjectPath + "/Assets/StreamingAssets/resources/scriptobject";
    private static string m_strConfigAssetBundlePath = ProjectPath + "/scriptobject";
    private static string m_strScriptPath = ProjectPath + "/Assets/Scripts/StaticData/ScriptObject";
    private static string m_strLoaderPath = ProjectPath + "/Assets/AssetTemp";

    [MenuItem("[Tools]/配置表一键打包")]
    public static void Operate()
    {
        ClearTempFiles();
        string strCsvPath = ProjectPath + "Csv";
        FindCSV(strCsvPath);
        Write();
        m_lstCSV.Clear();
        SaveFileInfo();
        AssetDatabase.Refresh();
        EditorPrefs.SetBool("LoadOver", false);
    }
    
    [DidReloadScripts]
    public static void OnScriptsLoadOver()
    {
        if (CheckLoadOver()) return;
        LoadFileInfo();
        ConfigAssetBundleWriter.ExecutePath(m_strConfigAssetBundlePath, BuildTarget.StandaloneWindows, m_dictOutputTxt);
        m_dictOutputTxt.Clear();
        EditorPrefs.SetBool("LoadOver", true);
        EditorUtility.DisplayDialog("配置表打包", "打包完成", "确认");
        System.Diagnostics.Process.Start("explorer.exe", ResourcePath);
    }

    private static void FindCSV(string path)
    {
        if (!Directory.Exists(path))
        {
            Debug.LogError("csv不存在，请检查" + path + "路径");
            return;
        }

        try
        {
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] lstFile = di.GetFiles();
            foreach (FileInfo i in lstFile)
            {
                if (i.Extension == ".csv")
                {
                    if (File.Exists(i.FullName))
                    {
                        m_lstCSV.Add(i.FullName);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.Write(e.Message);
        }

        try
        {
            DirectoryInfo di = new DirectoryInfo(path);
            DirectoryInfo[] lstDire = di.GetDirectories();
            foreach (DirectoryInfo i in lstDire)
            {
                FindCSV(i.ToString());
            }
        }
        catch (Exception e)
        {
            Console.Write(e.Message);
        }
    }

    private static void Write()
    {

        for (int i = 0, count = m_lstCSV.Count; i < count; i++)
        {
            string strName = m_lstCSV[i];
            if (LoadCsv(strName))
            {
                string fileName = GetFileName(strName);
                Debug.Log("导入表:" + fileName + "成功！");
            }
        }
    }

    private static bool LoadCsv(string path)
    {
        bool ret = false;
        string fileName = GetFileName(path);
        if (fileName.StartsWith("c_"))
        {
            ret = ConvertCsv(path, ProjectPath);
        }

        return ret;
    }

    private static bool ConvertCsv(string inputFile, string outputFile)
    {
        if (Path.GetExtension(inputFile) != ".csv")
        {
            return false;
        }

        bool bResult = true;


        try
        {
            CsvStreamReader csvReader = new CsvStreamReader(inputFile, Encoding.Default, true);
            bResult &= csvReader.LoadCsvFile();

            string strTxtName = GetFileName(inputFile);
            string strOutputTxtName = m_strTxtPath + "/" + strTxtName + ".txt";
            CTxtWriter txtWriter = new CTxtWriter(csvReader, strOutputTxtName, strTxtName);
            bResult &= txtWriter.WriteTxtFile();
            
            if (bResult)
            {
                m_dictOutputTxt.Add(strTxtName, "Assets/MyResources/outputtxt/" + strTxtName + ".txt");
                string strOutputScriptName = outputFile + "/Assets/Scripts/StaticData/ScriptObject/" + CScriptWriter.GetScriptName(strTxtName) + ".cs";
                CScriptWriter scriptWriter = new CScriptWriter(csvReader, strOutputScriptName, strTxtName);
                bResult &= scriptWriter.WriteScriptFile();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("导入表" + System.IO.Path.GetFileName(inputFile) + "失败:" + ex.Message);
            bResult = false;
        }
        return bResult;
    }
    
    private static string ProjectPath
    {
        get
        {
            string strTemp = Application.dataPath;
            strTemp = strTemp.Replace("Assets", "");
            return strTemp;
        }
        
    }

    public static string GetFileName(string path)
    {
        FileInfo fi = new FileInfo(path);
        string retVal = fi.Name;
        int index = retVal.LastIndexOf('.');
        retVal = retVal.Substring(0, index);
        return retVal;
    }

    private static void SaveFileInfo()
    {
        EditorPrefs.SetInt("count", m_dictOutputTxt.Count);
        int nIndex = 0;
        foreach(var v in m_dictOutputTxt)
        {
            EditorPrefs.SetString(nIndex.ToString(), v.Key);
            nIndex++;
            EditorPrefs.SetString(v.Key, v.Value);
        }
        m_dictOutputTxt.Clear();
    }

    private static void LoadFileInfo()
    {
        int nCount = EditorPrefs.GetInt("count");
        for (int i = 0; i < nCount; i++)
        {
            string strKey = EditorPrefs.GetString(i.ToString());
            string strValue = EditorPrefs.GetString(strKey);
            m_dictOutputTxt.Add(strKey, strValue);
            EditorPrefs.DeleteKey(strKey);
        }
        //EditorPrefs.DeleteAll();
    }

    private static void ClearTempFiles()//删除所有临时文件
    {
        DeleteAllFile(m_strTxtPath);
        DeleteAllFile(m_strConfigAssetBundlePath);
        DeleteAllFile(m_strLoaderPath);
        DeleteAllFile(m_strScriptPath);
    }

    private static bool DeleteAllFile(string fullPath)
    {
        //获取指定路径下面的所有资源文件  然后进行删除
        if (Directory.Exists(fullPath))
        {
            DirectoryInfo direction = new DirectoryInfo(fullPath);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                //if (files[i].Name.EndsWith(".meta"))
                //{
                //    continue;
                //}
                string FilePath = fullPath + "/" + files[i].Name;
                File.Delete(FilePath);
            }
            return true;
        }
        return false;
    }

    private static bool CheckLoadOver()
    {
        bool bLoadOver = false;
        if(EditorPrefs.HasKey("LoadOver"))
        {
            bLoadOver = EditorPrefs.GetBool("LoadOver");
        }
        return bLoadOver;
    }

    private static string ResourcePath
    {
        get
        {
            //string strPath = ProjectPath + "Assets/StreamingAssets/resources/scriptobject";
            string strPath = ProjectPath + "scriptobject";
            strPath = strPath.Replace("/", "\\");
            return strPath;
        }
    }

}
