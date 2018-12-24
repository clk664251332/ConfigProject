using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game.Config
{
    [Serializable]
    public class ScriptableObjectBase : ScriptableObject
    {
        protected bool m_bFillDic = false;
        public virtual T GetData<T>(uint key) where T : class
        {
            return default(T);
        }

        public virtual void FillDic()
        {
            CustomFillDic();
        }

        public virtual void CustomFillDic() { }

        public virtual void ParseData() { }

        public virtual List<T> GetValueList<T>() where T : class
        {
            return null;
        }
    }

    [Serializable]
    public class ObjectBase
    {
        public virtual void ParseData() { }
    }
}

