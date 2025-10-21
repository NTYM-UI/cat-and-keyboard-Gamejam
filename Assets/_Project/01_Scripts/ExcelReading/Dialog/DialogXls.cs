using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using UnityEngine;
namespace XlsWork.Dialogs
{
    public class DialogXls : MonoBehaviour
    {
        // 单例模式，确保表格只加载一次
        private static Dictionary<int, DialogItem> dialogDict;

        public static Dictionary<int, DialogItem> LoadDialogAsDictionary()
        {
            if (dialogDict != null) return dialogDict; // 已加载则直接返回

            dialogDict = new Dictionary<int, DialogItem>();

            string path = Application.streamingAssetsPath + "/Excel/Dialog.xlsx"; //指定表格的文件路径。在编辑器模式下，Application.dataPath就是Assets文件夹

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            ExcelPackage excel = new ExcelPackage(fs);
            ExcelWorksheet sheet = excel.Workbook.Worksheets[1]; // 读取第一个工作表

            int rowCount = sheet.Dimension.End.Row;//工作表的行数

            // 从第2行开始读取（表格第1行是表头）
            for (int row = 2; row <= rowCount; row++)
            {
                DialogItem item = new DialogItem();

                // 解析表格列（A列到H列，对应1-8）
                item.flag = sheet.Cells[row, 1].Text; // A列：标志
                item.id = int.Parse(sheet.Cells[row, 2].Text); // B列：ID
                item.character = sheet.Cells[row, 3].Text; // C列：人物
                item.position = sheet.Cells[row, 4].Text; // D列：位置
                item.content = sheet.Cells[row, 5].Text; // E列：内容

                // 处理跳转ID（可能为空，需容错）（当字符串为空时，int.TryParse会返回false，jumpID的返回值为0）
                if (int.TryParse(sheet.Cells[row, 6].Text, out int jumpId))
                    item.jumpId = jumpId;
                item.effect = sheet.Cells[row, 7].Text; // G列：效果
                item.target = sheet.Cells[row, 8].Text; // H列：目标

                dialogDict.Add(item.id, item); // 存入字典（ID为键）
            }
            Debug.Log("对话表格加载完成，共" + dialogDict.Count + "条数据");
            return dialogDict;
        }
    }
}