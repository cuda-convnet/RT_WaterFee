using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace WHC.Framework.BaseUI
{
    /// <summary>
    /// �ֶ���������
    /// </summary>
    [Serializable]
    public enum FieldType
    {
        Text = 0,
        Numeric = 1,
        DateTime = 2,
        DropdownList = 3
    }
}
