using Domain.Model;
using GemBox.Spreadsheet;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExcelParser
{
    public static class SitichkoExcelParser
    {
        //эта хрень преобразует xls файл ситичка (который, как оказывается, на самом деле явлчется html) в нормальный xlsx
        private static FileInfo converter_html_to_xlsx(FileInfo fileInfo)
        {
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");

            ExcelFile.Load(fileInfo.FullName, LoadOptions.HtmlDefault).Save(fileInfo.Name + ".xlsx");

            File.Delete(fileInfo.FullName);

            return new FileInfo(fileInfo.Name + ".xlsx");
        }

        public static Test Parser(FileInfo fileInfo)
        {
            fileInfo = converter_html_to_xlsx(fileInfo);
            var package = new ExcelPackage(fileInfo);
            Test test = new Test();
            var sheet2 = package.Workbook.Worksheets[0];
            test.Name = getlistname(sheet2.Cells[1, 1].Value.ToString());
            test.Comment = new string(((string)sheet2.Cells[2, 1].Value).Skip(10).ToArray());
            string text = null;

            test.Date = DateTime.Now;
            test.Questions = new List<Question>();

            for (int i = 1; ;) // по логике сей код должен сливать воедино все вопросы так, как буд-то разделения по группам и не было
            {
                // проверь правда ли это знак номера
                i = findr_excel("№", sheet2, i, 1) + 1; // в теории здесь может быть ошибка, если попадется лишняя строка, состоящая чисто из "№"(номера)
                if (i < 1) break;
                for (; !string.IsNullOrEmpty(text = sheet2.Cells[i, 3].Value?.ToString()); i++)
                {
                    test.Questions.Add(new Question()
                    {
                        question = text,
                        ExpectedResult = sheet2.Cells[i, 4].Value.ToString()
                    });

                }
            }


            File.Delete(fileInfo.FullName);
            
            return test;
        }
        private static int findr_excel(string str, OfficeOpenXml.ExcelWorksheet s, int r = 0, int c = 0)
        {
            string text = null;
            for (; !string.IsNullOrEmpty(text = s.Cells[r, c].Value?.ToString()); r++)
            {
                if (text == str) return r;
            }

            return -1;
        }

        private static string getlistname(string str)
        {
            int endindex = str.IndexOf(':');
            return str.Substring(endindex+1);
        }
    }
}
