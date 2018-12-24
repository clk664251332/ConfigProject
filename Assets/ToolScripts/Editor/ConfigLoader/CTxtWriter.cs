using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CTxtWriter {

    private CsvStreamReader m_csvReader;
    private string m_strOutputFile;
    private string m_strName;
    private string[] m_arrVariables;
    private string[] m_arrTypes;
    private string[] m_arrNotes;
    private List<string> m_lstIDs = new List<string>();

    public bool WriteTxtFile()
    {
        m_arrVariables = GetLine(1);//配置表第一行为英文变量
        m_arrTypes = GetLine(2);    //配置表第二行为类型
        m_arrNotes = GetLine(3);    //配置表第三行为中文描述
        if (!CheckValueType()) return false;

        FileStream fs = new FileStream(m_strOutputFile, FileMode.OpenOrCreate, FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

        sw.WriteLine("{");
        sw.WriteLine("    \"datas\": [");
        bool bContinue = true;
        for (int i = 4; i <= m_csvReader.RowCount && bContinue; i++)//第四行开始为数据项
        {
            sw.WriteLine("        {");
            for (int j = 1; j <= m_csvReader.ColCount; j++)
            {
                string content = Get_uft8(m_csvReader[i, j].ToString());

                if (!UtilConfig.CheckValue(content, m_arrTypes[j - 1]))//检查数据是否符合对应类型
                {
                    bContinue = false;
                    Debug.LogError("导入表:" + m_strName + "第" + i + "行,第" + j + "列数据项" + content + "不符合类型");
                    break;
                }
                
                if(j == 1)//ID的唯一性检查
                {
                    if (CheckIDUniqueness(content))
                    {
                        bContinue = false;
                        Debug.LogError("导入表:" + m_strName + "第" + i + "行的ID值不唯一");
                        break;
                    }
                }

                bool hasYinhao = false;
                if (-1 != content.IndexOf("\r") || -1 != content.IndexOf("\n"))
                {
                    hasYinhao = true;
                }

                string fmt;

                // "{0}"\t
                fmt = String.Format("{0}{1}0{2}{3}{4}", hasYinhao ? "\"" : "",
                "{", "}", hasYinhao ? "\"" : "", j + 1 == m_csvReader.ColCount + 1 ? "" : "\t");

                if (IsStringType(m_arrTypes[j - 1]))
                    content = DealWithStringType(content);

                if (j == m_csvReader.ColCount)
                    sw.WriteLine(fmt, "            \"" + m_arrVariables[j - 1] + "\": " + content);
                else
                    sw.WriteLine(fmt, "            \"" + m_arrVariables[j - 1] + "\": " + content + ",");
            }

            if (i == m_csvReader.RowCount)
                sw.WriteLine("        }");
            else
                sw.WriteLine("        },");
        }
        sw.WriteLine("    ]");
        sw.Write("}");
        sw.Flush();
        sw.Close();
        m_lstIDs.Clear();
        return bContinue;
    }

    public CTxtWriter(CsvStreamReader csvReader, string outputFile, string strName)
    {
        this.m_csvReader = csvReader;
        this.m_strOutputFile = outputFile;
        this.m_strName = strName;
        if (File.Exists(m_strOutputFile))
        {
            File.Delete(m_strOutputFile);
        }
    }

    private string[] GetLine(int nRow)
    {
        List<String> lstString = new List<string>();
        for (int i = 1; i <= m_csvReader.ColCount; i++)
        {
            lstString.Add(m_csvReader[nRow, i]);
        }
        return lstString.ToArray();
    }

    private string Get_uft8(string unicodeString)
    {
        UTF8Encoding utf8 = new UTF8Encoding();
        Byte[] encodedBytes = utf8.GetBytes(unicodeString);
        String decodedString = utf8.GetString(encodedBytes);
        return decodedString;
    }

    private bool CheckValueType()
    {
        if (!UtilConfig.CheckIDType(m_arrTypes[0]))//ID合法性检查
        {
            Debug.LogError("导入表:" + m_strName + "ID数据类型配置" + m_arrTypes[0] + "有误");
            UtilConfig.LogIDTypes();
            return false;
        }
        bool bTypeConfig = true;
        for (int i = 0; i < m_arrTypes.Length; i++)
        {
            if (!UtilConfig.CheckType(m_arrTypes[i]))//数值类型输入不符合
            {
                bTypeConfig = false;
                Debug.LogError("导入表:" + m_strName + "变量" + m_arrVariables[i] + "类型配置" + m_arrTypes[i] + "有误");
                UtilConfig.LogAllTypes();
            }
        }
        return bTypeConfig;
    }

    private string DealWithStringType(string strOld)
    {
        string strNew = "\"" + strOld + "\"";
        return strNew;
    }

    private bool IsStringType(string strType)
    {
        if (strType != null && strType == "string")
            return true;
        else
            return false;
    }

    private bool CheckIDUniqueness(string strID)
    {
        bool bUnique = m_lstIDs.Contains(strID);
        if (!bUnique)
            m_lstIDs.Add(strID);
        return bUnique;
    }

}
