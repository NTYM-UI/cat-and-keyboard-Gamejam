using UnityEngine;
using UnityEditor;
using System.IO;
using OfficeOpenXml;//启用EPPlus插件

public class ReadUnits : MonoBehaviour
{
    [MenuItem("Excel/Read Excel")]//添加Unity编辑器菜单项用来读表
    static void LoadExcel()
    {
        string path = Application.dataPath + "/Excel/Unit.xlsx";//指定待读取表格的文件路径。在编辑器模式下，Application.dataPath就是Assets文件夹

        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);//建立文件流fs

        ExcelPackage excel = new ExcelPackage(fs);//这是来自第三方插件的功能，将文件流fs视为Excel文件，开始访问

        ExcelWorksheets workSheets = excel.Workbook.Worksheets;//查找到工作簿内的各工作表

        ExcelWorksheet workSheet = workSheets[1];//只看第一个工作表，余者不看

        int colCount = workSheet.Dimension.End.Column;//工作表的列数
        int rowCount = workSheet.Dimension.End.Row;//工作表的行数

        for (int row = 1; row <= rowCount; row++)//从当前工作表的第一行遍历到最后一行
        {
            for (int col = 1; col <= colCount; col++)//从第一列遍历到最后一列
            {
                string text = workSheet.Cells[row, col].Text;//读取每个单元格中的数据
                Debug.LogFormat("表格坐标:({0},{1}),表格内容:{2}", row, col, text);
            }
        }

        Debug.Log("complete");
        return;
    }
}