using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnitInfo))]//将本模块指定为UnitInfo组件的编辑器自定义模块
public class UnitXls_Editor : Editor
{
    public override void OnInspectorGUI()//对UnitInfo在Inspector中的绘制方式进行接管
    {
        DrawDefaultInspector();//绘制常规内容

        if (GUILayout.Button("从配表ID刷新"))//添加按钮和功能——当组件上的按钮被按下时
        {
            UnitInfo unitInfo = (UnitInfo)target;
            unitInfo.InitSelf();//令组件调用自身的InitSelf方法
        }
    }
}