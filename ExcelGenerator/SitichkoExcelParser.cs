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
        private static FileInfo test_converter_html_to_xlsx(FileInfo fileInfo)
        {
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");

            ExcelFile.Load(fileInfo.FullName, LoadOptions.HtmlDefault).Save(fileInfo.Name + ".xlsx");

            File.Delete(fileInfo.FullName);

            return new FileInfo(fileInfo.Name + ".xlsx");
        }

        public static Test Parser(String filepath)
        {
            FileInfo fileInfo = new FileInfo(filepath);

            if (!fileInfo.Exists) return null;

            fileInfo = test_converter_html_to_xlsx(fileInfo);

            var package = new ExcelPackage(fileInfo);
            Test test = new Test();
            var sheet2 = package.Workbook.Worksheets[0];

            test.Name = getlistname(sheet2.Cells[1, 1].Value.ToString());

            test.Comment = new string(((string)sheet2.Cells[2, 1].Value).Skip(10).ToArray());

            string text = null;

            test.Questions = new List<Question>();
            for (int i = findr_excel("Проверка", sheet2, 6, 3) + 1; !string.IsNullOrEmpty(text = sheet2.Cells[i, 3].Value?.ToString()); i++)
            // может произойти ошибка из-за int i = findr_excel(...     (мне лень исправлять)
            {
                test.Questions.Add(new Question()
                {
                    question = text,
                    ExpectedResult = sheet2.Cells[i, 4].Value.ToString()
                });

            }

            test.Date = DateTime.Now;

            File.Delete(fileInfo.FullName);
            
            return test;
        }
        private static int findr_excel(string str, OfficeOpenXml.ExcelWorksheet s, int r = 0, int c = 0)
        {
            string text = null;
            for (int i = r; !string.IsNullOrEmpty(text = s.Cells[i, c].Value.ToString()); i++)
            {
                if (text == str) return i;
            }

            return -1;
        }

        private static string getlistname(string str)
        {
            //string abr = "";
            //str = str.Skip("Название ".Length).ToString();
            int endindex = str.IndexOf(':');
            return str.Substring(endindex+1);

            //for (int i = 0; i < endindex; i++)
            //{
            //    abr += str[i];
            //}
        }
    }
}
