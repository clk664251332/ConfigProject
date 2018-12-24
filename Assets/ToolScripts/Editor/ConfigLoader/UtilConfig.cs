using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UtilConfig : MonoBehaviour {

    private static string[] m_arrTypes = { "string", "uint", "int", "float", "double", "ulong", "long" };

    public static bool CheckType(string strType)
    {
        if (m_arrTypes.Contains(strType))
            return true;
        else
            return false;
    }

    public static bool CheckIDType(string strIDType)
    {
        if (strIDType.Equals(m_arrTypes[0]) || strIDType.Equals(m_arrTypes[1]))
            return true;
        else
            return false;
    }

    public static bool CheckValue(string strValue, string strType)
    {
        if (!CheckType(strType)) return false;

        bool bResult = false;
        switch (strType)
        {
            case "string":
                {
                    bResult = true;
                    break;
                }
            case "uint":
                {
                    uint uTemp;
                    bResult = uint.TryParse(strValue, out uTemp);
                    break;
                }
            case "int":
                {
                    int nTemp;
                    bResult = int.TryParse(strValue, out nTemp);
                    break;
                }
            case "float":
                {
                    float fTemp;
                    bResult = float.TryParse(strValue, out fTemp);
                    break;
                }
            case "double":
                {
                    double fTemp;
                    bResult = double.TryParse(strValue, out fTemp);
                    break;
                }
            case "ulong":
                {
                    ulong uTemp;
                    bResult = ulong.TryParse(strValue, out uTemp);
                    break;
                }
            case "long":
                {
                    long nTemp;
                    bResult = long.TryParse(strValue, out nTemp);
                    break;
                }
            default:
                {
                    bResult = false;
                    break;
                }
        }
        return bResult;
    }

    public static void LogAllTypes()
    {
        StringBuilder strLog = new StringBuilder("允许配置数据类型有：");
        for (int i = 0; i < m_arrTypes.Length; i++)
        {
            strLog.Append(m_arrTypes[i] + ",");
        }
        strLog.Append("请检查对应配置表类型");
        Debug.Log(strLog.ToString());
    }

    public static void LogIDTypes()
    {
        Debug.Log("允许配置ID的数据类型有uint，string，请检查对应配置表ID数据类型");
    }

}
