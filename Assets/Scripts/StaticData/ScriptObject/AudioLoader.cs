using UnityEngine;
using System;
using System.Collections.Generic;

namespace Game.Config
{
    [Serializable]
    [ConfigName("c_audio")]
    public partial class AudioLoader : ScriptableObjectBase
    {
        [Serializable]
        public partial class Data : ObjectBase
        {
            public uint Id; // ID
            public string AudioName; // 音频名称
            public string Path; // 音频路径
            public string Descr; // 描述
        }

        public List<Data> datas = new List<Data>();

        private Dictionary<uint, Data> m_dicData = new Dictionary<uint, Data>();

        public override void FillDic()
        {
            base.FillDic();

            m_dicData.Clear();
            for (int i = 0, count = datas.Count; i < count; i++)
            {
                CommonHelper.FillDic<uint, Data>(datas[i].Id, datas[i], m_dicData);
            }
        }

        public override void ParseData()
        {
            base.ParseData();

            for (int i = 0, count = datas.Count; i < count; i++)
            {
                datas[i].ParseData();
            }
        }

        public override T GetData<T>(uint key)
        {
            Data data = null;
            if (m_dicData.TryGetValue(key, out data))
            {
                return data as T;
            }
            return default(T);
        }
    }
}
