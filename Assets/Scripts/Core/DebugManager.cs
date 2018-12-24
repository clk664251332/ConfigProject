using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;

public class DebugManager : SingletonObject<DebugManager>
{
    //其他平台需要日志请自行修改，不用上传

#if _DEBUG
    public static bool EnableLog = true;
#else
    public static bool EnableLog = true;
#endif

    private StreamWriter m_streamWriter = null;
    private bool m_steamDestroy = false;
    private Thread m_processThread = null;
    private static List<string> m_logList = new List<string>();
    private static Mutex mut = new Mutex();
    private static float m_writeTime;

    public void Init()
    {
        if (EnableLog)
        {
            try
            {
                Directory.CreateDirectory(SingletonObject<GameConfig>.GetInst().ResourcePath + "Logs/");

                m_streamWriter = new StreamWriter(SingletonObject<GameConfig>.GetInst().ResourcePath + DateTime.Now.ToString("dd-MM-yy") + ".log", true);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.ToString());
            }

            m_processThread = new Thread(new ThreadStart(ProcessLog));
            m_processThread.IsBackground = true;
            m_processThread.Start();

            Application.logMessageReceived += OnLog;
        }
    }

	void OnLog (string message, string stacktrace, LogType type)
	{
        if (type == LogType.Exception || type == LogType.Error)
		{
            if (!string.IsNullOrEmpty(stacktrace))
                LogFile(stacktrace);
		}

        LogFile(message);
	}

    public void Close()
    {
        WriteToFile();

        if (m_processThread != null)
        {
            m_processThread.Abort();
            m_processThread = null;
        }

        if (m_streamWriter != null)
        {
            m_steamDestroy = true;
            m_streamWriter.Close();
            m_streamWriter = null;
        }
    }

    void ProcessLog()
    {
        while (true)
        {
            try
            {
                if (m_logList.Count > 50)
                {
                    mut.WaitOne();
                    {
                        WriteToFile();
                    }
                    mut.ReleaseMutex();
                }

                System.Threading.Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.ToString());
            }
        }
    }

    public List<string> logList
    {
        get
        {
            return m_logList;
        }
    }

    void WriteToFile()
    {
        if (m_streamWriter != null && !m_steamDestroy)
        {
            if (m_logList.Count != 0)
            {
                for (int i = 0; i < m_logList.Count; i++)
                    m_streamWriter.WriteLine(m_logList[i]);

                m_streamWriter.Flush();
                m_logList.Clear();
            }
        }
    }

    public static void LogFile(object message)
    {
        if (EnableLog)
        {
            mut.WaitOne();
            {
                m_logList.Add(System.DateTime.Now.ToString("HH:mm:ss ffff") + "  " + message.ToString());
            }
            mut.ReleaseMutex();
        }
    }

    public static void LogTrack()
    {
        if (EnableLog)
        {
            UnityEngine.Debug.Log(StackTraceToString());
        }
    }

    public static string StackTraceToString()
    {
        string StackTrace = "";
        var frames = new System.Diagnostics.StackTrace().GetFrames();
        for (int i = 2; i < frames.Length; i++) /* Ignore current StackTraceToString method...*/
        {
            var currFrame = frames[i];
            var method = currFrame.GetMethod();
            StackTrace += string.Format("{0}:{1}\n",
                                        method.ReflectedType != null ? method.ReflectedType.Name : string.Empty,
                                        method.Name);
        }
        return StackTrace;
    }
}
