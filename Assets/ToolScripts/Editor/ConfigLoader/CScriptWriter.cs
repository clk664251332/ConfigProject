using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CScriptWriter {

    private CsvStreamReader m_csvReader;
    private string m_strOutputFile;
    private string m_strName;
    private string m_strScriptName;
    private string[] m_arrVariables;
    private string[] m_arrTypes;
    private string[] m_arrNotes;

    public bool WriteScriptFile()
    {
        m_arrVariables = GetLine(1);//配置表第一行为英文变量
        m_arrTypes = GetLine(2);    //配置表第二行为类型
        m_arrNotes = GetLine(3);    //配置表第三行为中文描述

        if (!CheckValueType()) return false;

        FileStream fs = new FileStream(m_strOutputFile, FileMode.OpenOrCreate, FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

        sw.WriteLine("using UnityEngine;");
        sw.WriteLine("using System;");
        sw.WriteLine("using System.Collections.Generic;");
        sw.WriteLine();
        sw.WriteLine("namespace Game.Config");
        sw.WriteLine("{");
        sw.WriteLine("    [Serializable]");
        sw.WriteLine("    [ConfigName(\"" + m_strName + "\")]");
        sw.WriteLine("    public partial class " + m_strScriptName + " : " + "ScriptableObjectBase");
        sw.WriteLine("    {");
        sw.WriteLine("        [Serializable]");
        sw.WriteLine("        public partial class Data : ObjectBase");
        sw.WriteLine("        {");
        for (int i = 0; i < m_arrVariables.Length; i++)
        {
            sw.WriteLine("            public " + m_arrTypes[i] + " " + m_arrVariables[i] + "; // " + m_arrNotes[i]);
        }
        sw.WriteLine("        }");
        sw.WriteLine();
        sw.WriteLine("        public List<Data> datas = new List<Data>();");
        sw.WriteLine();
        sw.WriteLine("        private Dictionary<" + m_arrTypes[0] + ", Data> m_dicData = new Dictionary<" + m_arrTypes[0] + ", Data>();");
        sw.WriteLine();
        sw.WriteLine("        public override void FillDic()");
        sw.WriteLine("        {");
        sw.WriteLine("            base.FillDic();");
        sw.WriteLine();
        sw.WriteLine("            m_dicData.Clear();");
        sw.WriteLine("            for (int i = 0, count = datas.Count; i < count; i++)");
        sw.WriteLine("            {");
        sw.WriteLine("                CommonHelper.FillDic<" + m_arrTypes[0] + ", Data>(datas[i].Id, datas[i], m_dicData);");
        sw.WriteLine("            }");
        sw.WriteLine("        }");
        sw.WriteLine();
        sw.WriteLine("        public override void ParseData()");
        sw.WriteLine("        {");
        sw.WriteLine("            base.ParseData();");
        sw.WriteLine();
        sw.WriteLine("            for (int i = 0, count = datas.Count; i < count; i++)");
        sw.WriteLine("            {");
        sw.WriteLine("                datas[i].ParseData();");
        sw.WriteLine("            }");
        sw.WriteLine("        }");
        sw.WriteLine();
        sw.WriteLine("        public override T GetData<T>(" + m_arrTypes[0] + " key)");
        sw.WriteLine("        {");
        sw.WriteLine("            Data data = null;");
        sw.WriteLine("            if (m_dicData.TryGetValue(key, out data))");
        sw.WriteLine("            {");
        sw.WriteLine("                return data as T;");
        sw.WriteLine("            }");
        sw.WriteLine("            return default(T);");
        sw.WriteLine("        }");
        sw.WriteLine("    }");
        sw.WriteLine("}");

        sw.Flush();
        sw.Close();

        return true;
    }

    public CScriptWriter(CsvStreamReader csvReader, string outputFile, string strName)
    {
        this.m_csvReader = csvReader;
        this.m_strOutputFile = outputFile;
        this.m_strName = strName;
        this.m_strScriptName = GetScriptName(strName);
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

    public static string GetScriptName(string strName)
    {
        strName = strName.Substring(1);
        string strScriptName = "";
        for (int i = 0; i < strName.Length; i++)
        {
            if (strName[i].Equals('_'))
            {
                strScriptName += Char.ToUpper(strName[i + 1]);
                i++;
            }
            else
            {
                strScriptName += strName[i];
            }
        }
        return strScriptName + "Loader";
    }

    private bool CheckValueType()
    {
        bool bTypeConfig = true;
        for (int i = 0; i < m_arrTypes.Length; i++)
        {
            if (!UtilConfig.CheckType(m_arrTypes[i]))//数值类型输入不符合
            {
                bTypeConfig = false;
                Debug.LogError("导入表:" + m_strName + "变量" + m_arrVariables[i] + "类型配置" + m_arrTypes[i] + "有误");
            }
        }
        return bTypeConfig;
    }

}
