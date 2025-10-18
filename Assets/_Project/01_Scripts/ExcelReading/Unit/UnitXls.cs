using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;
using OfficeOpenXml;

namespace XlsWork
{
    namespace UnitsXls
    {
        public class UnitXls : MonoBehaviour
        {
            /// <summary>
            /// 配表中属性字段的数量
            /// </summary>
            public static int CountOfAttributes = 4;

            public static Dictionary<int, IndividualData> LoadExcelAsDictionary()
            {
                Dictionary<int, IndividualData> ItemDictionary = new Dictionary<int, IndividualData>();//新建字典，用于存储以行为单位的各个操作单元

                string path = Application.dataPath + "/Excel/Unit.xlsx";//指定表格的文件路径。在编辑器模式下，Application.dataPath就是Assets文件夹

                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);//建立文件流fs

                ExcelPackage excel = new ExcelPackage(fs);

                ExcelWorksheets workSheets = excel.Workbook.Worksheets;//获取全部工作表

                ExcelWorksheet workSheet = workSheets[1];//只看第一个工作表，余者不看

                int colCount = workSheet.Dimension.End.Column;//工作表的列数
                int rowCount = workSheet.Dimension.End.Row;//工作表的行数

                for (int row = 2; row <= rowCount; row++)//从当前工作表的第二行遍历到最后一行(第一行是表头，所以不读取)
                {
                    IndividualData item = new IndividualData(CountOfAttributes);//新建一个操作单元，开始接收本行数据

                    for (int col = 1; col <= colCount; col++)//从第一列遍历到最后一列
                    {
                        //读取每个单元格中的数据
                        item.Values[col - 1] = workSheet.Cells[row, col].Text;//将单元格中的数据写入操作单元
                    }

                    int itemID = Convert.ToInt32(item.Values[0].ToString());//获取操作单元的ID

                    ItemDictionary.Add(itemID, item);//将ID和操作单元写入字典
                }

                Debug.Log("complete");
                return ItemDictionary;
            }
        }
    }
}