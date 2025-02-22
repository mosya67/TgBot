using Domain.Model;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using StatusGeneric;
using System.Collections.Generic;
using System.Linq;

namespace ExcelServices
{
    internal class WriteDataInExcel
    {
        private string[] NamesColumnsOfTest = {"дата тестирования", "тестировщик", "комментарий", "доп комментарий", "окружение", "сборка", "ПРОВЕРКА"};
        private string[] NamesColumnsOfTest2 = { "", "", "", "", "", "", "ОЖИДАЕМЫЙ РЕЗУЛЬТАТ" };
        internal IStatusGeneric<byte[]> Generate(IList<TestResult> results)
        {
            var status = new StatusGenericHandler<byte[]>();

            var package = new ExcelPackage();
            if (!status.HasErrors)
            {
                if (results == null || results?.Count() == 0)
                {
                    var sheet2 = package.Workbook.Worksheets.Add("Лист 1");
                    sheet2.Column(1).Width = 20;
                    sheet2.Cells[1, 1].Value = "результатов нет";
                    status.SetResult(package.GetAsByteArray());
                    return status;
                }

                var sheet = package.Workbook.Worksheets.Add(results[0].Test.Name);
                sheet.Column(1).Width = 20;
                sheet.Column(2).Width = 24;

                WriteNamesColumns(sheet, NamesColumnsOfTest, 0, 1);
                WriteNamesColumns(sheet, NamesColumnsOfTest2, 0, 2);

                WriteQuestions(sheet, results[0].Test, NamesColumnsOfTest.Length + 1, 1);
                WriteExpectedResult(sheet, results[0].Test, NamesColumnsOfTest.Length + 1, 2);

                for (int i = 0; i < results.Count(); i++)
                {
                    sheet.Column(i + 3).Width = 17;
                    WriteDataOfTest(sheet, results[i], i + 3);
                    WriteResults(sheet, results[i], i + 3, NamesColumnsOfTest.Length + 1);
                }
                status.SetResult(package.GetAsByteArray());
            }
            return status;
        }

        private void WriteDataOfTest(ExcelWorksheet sheet, TestResult res, int col)
        {
            sheet.Cells[1, col].Value = res.Date.ToShortDateString() + ' ' + res.Date.ToShortTimeString();

            sheet.Cells[2, col].Value = res.User.Fio; 
            sheet.Cells[2, col].Style.WrapText = true;

            sheet.Cells[3, col].Value = res.Comment; 
            sheet.Cells[3, col].Style.WrapText = true;

            sheet.Cells[4, col].Value = res.AdditionalComment; 
            sheet.Cells[4, col].Style.WrapText = true; // переход на следующую строчку

            sheet.Cells[5, col].Value = res.Apparat; 
            sheet.Cells[5, col].Style.WrapText = true;

            sheet.Cells[6, col].Value = res.Version;
            sheet.Cells[6, col].Style.WrapText = true;

            for (int i = 1; i <= NamesColumnsOfTest.Length; i++)
            {
                sheet.Cells[i, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[i, col].Style.Fill.BackgroundColor.SetColor(255, 248, 203, 173);
            }
        }

        private void WriteResults(ExcelWorksheet sheet, TestResult res, int col, int row)
        {
            for (int i = 0; i < res.Test.Questions.Count(); i++)
            {
                sheet.Cells[row + i, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                if (i >= res.Answers.Count())
                {
                    sheet.Cells[row + i, col].Value = "нет данных";
                    if (i % 2 != 0)
                        sheet.Cells[row + i, col].Style.Fill.BackgroundColor.SetColor(255, 214, 224, 242);
                    else
                        sheet.Cells[row + i, col].Style.Fill.BackgroundColor.SetColor(255, 180, 198, 231);
                    continue;
                }

                var result = res.Answers[i].Result;
                sheet.Cells[row + i, col].Value = result == "PASS" ? "✅" : result;

                if (result == "PASS") sheet.Cells[row + i, col].Style.Font.Color.SetColor(255, 0, 176, 80);
                else if (result == "BUG" || result == "BLOCKER") sheet.Cells[row + i, col].Style.Font.Color.SetColor(255, 255, 0 , 0);


                if (i % 2 != 0)
                    sheet.Cells[row + i, col].Style.Fill.BackgroundColor.SetColor(255, 214, 224, 242);
                else
                    sheet.Cells[row + i, col].Style.Fill.BackgroundColor.SetColor(255, 180, 198, 231);

                WriteComment(sheet, res.Answers.Select(e => e.Comment).ToArray()[i], res.User.Fio, row + i, col);
            }
        }

        private void WriteQuestions(ExcelWorksheet sheet, Test test, int row, int col)
        {
            for (int i = 0; i < test.Questions.Count(); i++)
            {
                sheet.Cells[row + i, col].Style.WrapText = true;


                sheet.Cells[row + i, col].Value = test.Questions[i].question;
                sheet.Cells[row + i, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                if (i % 2 != 0)
                    sheet.Cells[row + i, col].Style.Fill.BackgroundColor.SetColor(255, 159, 183, 225);
                else
                    sheet.Cells[row + i, col].Style.Fill.BackgroundColor.SetColor(255, 121, 154, 213);

                WriteComment(sheet, test.Questions.Select(e => e.Comment).ToList()[i], "admin", row + i, col);
            }
        }

        private void WriteExpectedResult(ExcelWorksheet sheet, Test test, int row, int col)
        {
            for (int i = 0; i < test.Questions.Count(); i++)
            {
                sheet.Cells[row + i, col].Style.WrapText = true;
                sheet.Cells[row + i, col].Value = test.Questions[i].ExpectedResult;
                sheet.Cells[row + i, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                if (i % 2 != 0)
                    sheet.Cells[row + i, col].Style.Fill.BackgroundColor.SetColor(255, 159, 183, 225);
                else
                    sheet.Cells[row + i, col].Style.Fill.BackgroundColor.SetColor(255, 121, 154, 213);
            }
        }

        private void WriteComment(ExcelWorksheet sheet, string str, string author, int row, int col)
        {
            if (sheet.Cells[row, col].Comment != null || string.IsNullOrWhiteSpace(str))
                return;
            sheet.Cells[row, col].AddComment(str, author);
            sheet.Cells[row, col].Comment.Visible = false;
        }

        private void WriteNamesColumns(ExcelWorksheet sheet, IList<string> names, int rowoffset, int row)
        {
            rowoffset++;
            for (int i = 0; i < names.Count(); i++)
            {
                sheet.Cells[rowoffset + i, row].Value = names[i];
                sheet.Cells[rowoffset + i, row].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[rowoffset + i, row].Style.Fill.BackgroundColor.SetColor(255, 242, 160, 110);
            }
        }
    }
}