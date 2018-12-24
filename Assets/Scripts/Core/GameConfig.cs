using UnityEngine;
using System.IO;

//客户端路径，版本号，网络相关配置
public class GameConfig : SingletonObject<GameConfig>
{
    #region Public Code-only Parameters
    public bool PublishVersion = false;//是否正式版本
    public bool DebugMode = true;//是否调试模式,影响日志打印,登陆出错提示等   
    public int ApacheConnectNum = 10;//同时下载的连接数
    public string VersionReleaseURL = "http://192.168.235.50:3000/update/version";
    public string VersionDebugURL = "http://192.168.235.50:3000/dev/update/version";
    public int GameGuardkey1;//防外挂秘钥1
    public int GameGuardkey2;//防外挂秘钥2
    #endregion

    #region Private Protected Properties
    //资源服务器地址
    private string m_strResourceURL;
    private string m_strAppURL;
    private string m_strServerID;
    private string m_strVersionName = "0.18.1";//版本号
    private string m_strResourceName = "0.18.1";//资源版本
    private int m_nMainVersion = 0;
    private int m_nDataVersion = 100;
    private int m_nAccountPort; //账号端口
    private string m_nAccountIP;  //账号IP
    #endregion

    #region attr
    public string ResourceIP
    {
        get
        {
            return m_strResourceURL;
        }
        set
        {
            m_strResourceURL = value;

            if (Application.platform != RuntimePlatform.WindowsPlayer && PublishVersion)
            {
                Security.PrefetchSocketPolicy(m_strResourceURL, 843);
            }
        }
    }

    public string AppURL
    {
        get
        {
            return m_strAppURL;
        }
        set
        {
            m_strAppURL = value;
        }
    }

    public string ServerID
    {
        get
        {
            return m_strServerID;
        }
        set
        {
            m_strServerID = value;
        }
    }

    public string VersionName
    {
        get
        {
            return m_strVersionName;
        }
        set
        {
            m_strVersionName = value;
        }
    }

    public string ResoureName
    {
        get
        {
            return m_strResourceName;
        }
        set
        {
            m_strResourceName = value;
        }
    }

    public int MainVersion
    {
        get
        {
            return m_nMainVersion;
        }
        set
        {
            m_nMainVersion = value;
        }
    }

    public int DataVersion
    {
        get
        {
            return m_nDataVersion;
        }

        set
        {
            m_nDataVersion = value;
        }
    }

    public string AccountIP
    {
        get
        {
            if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.OSXEditor)
            {
                return "192.168.235.50";
            }
            else
            {
                return "192.168.235.50";
            }
        }
        set
        {
            m_nAccountIP = value;
            if (Application.platform != RuntimePlatform.WindowsPlayer)
            {
                Security.PrefetchSocketPolicy(m_nAccountIP, 843);
            }
        }
    }

    public int AccountPort
    {
        get
        {
            return 3014;
        }
        set
        {
            m_nAccountPort = value;
        }
    }

    //资源数据地址
    public string StreamingPath
    {
        get
        {
#if UNITY_EDITOR
            return Application.streamingAssetsPath;
#elif UNITY_STANDALONE_WIN
                 return Application.dataPath + "/Data/";
#elif UNITY_ANDROID
                 return Application.persistentDataPath + "/";
#elif UNITY_IPHONE
                 return Application.persistentDataPath + "/object.data/";
#else
                 return Application.dataPath 
#endif
        }
    }

    //资源数据地址
    public string ResourcePath
    {
        get
        {
#if UNITY_EDITOR
            return Application.dataPath.Replace("/Assets", "/Data/");
#elif UNITY_STANDALONE_WIN
                 return Application.dataPath + "/Data/";
#elif UNITY_ANDROID
                 return Application.persistentDataPath + "/";
#elif UNITY_IPHONE
                 return Application.persistentDataPath + "/object.data/";
#else
                 return Application.dataPath 
#endif
        }
    }

    //资源数据地址
    public string ResourceExtension
    {
        get
        {
#if UNITY_ANDROID
            return ".android";
#elif UNITY_EDITOR
            if (Application.platform == RuntimePlatform.WindowsEditor)
                return "";
            else if (Application.platform == RuntimePlatform.OSXEditor)
                return ".iphone";
            else
                return "";
#elif UNITY_FLASH
            return ".swf";
#elif UNITY_STANDALONE_WIN
            return "";
#elif UNITY_IPHONE
            return ".iphone";
#else
            return "";
#endif
        }
    }
    #endregion

}      