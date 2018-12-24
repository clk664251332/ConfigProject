using System;
using System.IO;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
public class ToolCommon
{
    public static void ClearBuild(string path)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        if (dirInfo.Exists)
        {
            foreach (var file in dirInfo.GetFiles())
            {
                if (Path.GetExtension(file.Name) == ".manifest")
                {
                    File.Delete(file.FullName);
                }
                else if (Path.GetExtension(file.Name) == ".meta")
                {
                    File.Delete(file.FullName);
                }
                else if (Path.GetFileNameWithoutExtension(file.Name) == Path.GetFileNameWithoutExtension(file.DirectoryName))
                {
                    File.Delete(file.FullName);
                }
            }

            foreach (var directory in dirInfo.GetDirectories())
            {
                ClearBuild(directory.FullName);
            }
        }
    }

    public static bool ClearDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            try
            {
                Directory.Delete(path, true);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return false;
            }
        }
        return true;
    }

    public static void ClearFiles(string path)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);

        FileInfo[] files = dirInfo.GetFiles();   // 获取该目录下的所有文件

        foreach (FileInfo file in files)
        {
            file.Delete();
        }
    }

    public static void CreatePath(string path)
    {
        string NewPath = path.Replace("\\", "/");

        string[] strs = NewPath.Split('/');
        string p = "";

        for (int i = 0; i < strs.Length; ++i)
        {
            p += strs[i];

            if (i != strs.Length - 1)
            {
                p += "/";
            }

            if (!Path.HasExtension(p))
            {
                if (!Directory.Exists(p))
                    Directory.CreateDirectory(p);
            }

        }
    }

    public static string GetAssetPath(string windowPath)
    {
        int index = windowPath.IndexOf("/Assets/");
        return windowPath.Substring(index + 1, windowPath.Length - index - 1);
    }

    public static string GetFileMD5(string fileName)
    {
        try
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
        }
    }

    public static string GetWindowPath(string srcPath, string entension)
    {
        string dictName = Path.GetDirectoryName(srcPath);
        string FileName = Path.GetFileNameWithoutExtension(srcPath);

        string dstpath = "Assetbundles/" + dictName + "/" + FileName + entension;
        dstpath = dstpath.Replace("\\", "/");
        dstpath = dstpath.ToLower();
        return dstpath;
    }
}
#endif