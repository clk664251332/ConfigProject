using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommonHelper
{
    public static void FillDic<T1, T2>(T1 key, T2 value, Dictionary<T1, T2> dictData)
    {
        if (!dictData.ContainsKey(key))
        {
            dictData.Add(key, value);
        }
        else
        {
            dictData[key] = value;
        }
    }

    public static double GetDouble(string s)
    {
        double value = 0;
        double.TryParse(s, out value);

        return value;
    }

    public static float GetSingle(string s)
    {
        float value = 0;
        float.TryParse(s, out value);

        return value;
    }

    public static ulong GetULong(string s)
    {
        ulong value = 0;

        ulong.TryParse(s, out value);
        return value;
    }

    public static bool GetBoolean(int s)
    {
        bool result = ((s == 0) ? false : true);

        return result;
    }

    public static bool GetBoolean(uint s)
    {
        bool result = ((s == 0) ? false : true);

        return result;
    }

    public static byte GetByte(string s)
    {
        byte value = 0;
        byte.TryParse(s, out value);

        return value;
    }

    public static byte GetByte(ref byte src, string s)
    {
        if (src != 0)
        {
            return src;
        }

        if (s.Equals("0"))
        {
            return src;
        }

        byte value = 0;
        byte.TryParse(s, out value);

        src = value;
        return value;
    }
}