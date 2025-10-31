using UnityEngine;
using System;
using XlsWork;
using XlsWork.UnitsXls;

[Serializable]
public class UnitSettings
{
    public int ID;
    public string Name;
    public string describe;
    public string connotation;
}

public class UnitInfo : MonoBehaviour
{
    public UnitSettings Settings;

    [Header("配表内ID")]
    public int InitFromID;

    // 自动初始化
    private void Awake()
    {
        InitSelf();
    }

    public void InitSelf()//调用此方法即可读取配表对应ID内容
    {
        Action init;

        var dictionary = UnitXls.LoadExcelAsDictionary();//调用读表方法并获取生成的字典

        //如果字典中没有查到所需的ID，说明表内没有相应ID的数据，报出异常
        if (!dictionary.ContainsKey(InitFromID))
        {
            Debug.LogErrorFormat("未能在配表中找到指定的ID:{0}", InitFromID);
            return;
        }
        IndividualData item = dictionary[InitFromID];//如果字典中查到了所需的数据，则将该操作单元记录下来


        //将操作单元内的数据应用到自身
        //System.Convert在这里用于实现表格内文本对代码内数据类型的自适应，将Excel单元格中的字符串转换成int或其它类型
        init = (() =>
        {
            Settings.ID = Convert.ToInt32(item.Values[0]);
            Settings.Name = Convert.ToString(item.Values[1]);
            Settings.describe = Convert.ToString(item.Values[2]);
            Settings.connotation = Convert.ToString(item.Values[3]);
        });

        init();
    }
}