using Domain.Model;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using StatusGeneric;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ExcelServices
{
    internal class WriteDataInExcel
    {
        private string[] NamesColumnsOfTest = {"Testing date", "Tester", "Comment", "Addit. Comment", "Apparat", "Version"};
        internal IStatusGeneric<byte[]> Generate(IList<TestResult> testResults)
        {
            var status = new StatusGenericHandler<byte[]>();
            var tests = testResults.Select(e => e.Test).GroupBy(e => e.Id).Select(e => e.First()).ToList();

            if (tests.Count() == 0)
                status.AddError("ни одного теста не найдено");

            var package = new ExcelPackage();
            if (!status.HasErrors)
            {
                for (int i = 0; i < tests.Count(); i++)
                {
                    var sheet = package.Workbook.Worksheets.Add(tests[i].Name);
                    var results = testResults.Where(e => e.Test.Id == tests[i].Id).ToList();
                    sheet.Column(1).Width = 15;
                    WriteNamesColumns(sheet, NamesColumnsOfTest, 0);
                    for (int j = 0; j < results.Count(); j++)
                    {
                        WriteQuestions(sheet, results[j], 7, 1);
                        WriteDataOfTest(sheet, results[j], j + 1);
                        WriteResults(sheet, results[j], 2 + j);
                    }
                }
                status.SetResult(package.GetAsByteArray());
            }
            return status;
        }

        private void WriteDataOfTest(ExcelWorksheet sheet, TestResult res, int coloffset)
        {
            coloffset++;
            sheet.Cells[1, coloffset].Value = res.Date.ToShortDateString();
            sheet.Cells[2, coloffset].Value = res.User.Fio;
            sheet.Cells[3, coloffset].Value = res.Comment;
            sheet.Cells[4, coloffset].Value = res.AdditionalComment;
            sheet.Cells[5, coloffset].Value = res.Apparat;
            sheet.Cells[6, coloffset].Value = res.Version;
            for (int i = 1; i < 7; i++)
            {
                sheet.Cells[i, coloffset].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[i, coloffset].Style.Fill.BackgroundColor.SetColor(255, 248, 203, 173);
            }
        }

        private void WriteResults(ExcelWorksheet sheet, TestResult res, int col, int row = 7)
        {
            for (int i = 0; i < res.Answers.Count(); i++)
            {
                var result = res.Answers.ToList()[i].Result;
                sheet.Cells[row + i, col].Value = result == "PASS" ? "✅" : result == "BUG" ? "❌" : result;
                if (result == "PASS") sheet.Cells[row + i, col].Style.Font.Color.SetColor(255, 0, 176, 80);
                else if (result == "BUG") sheet.Cells[row + i, col].Style.Font.Color.SetColor(255, 255, 0 , 0);
                sheet.Cells[row + i, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                if (i % 2 != 0)
                    sheet.Cells[row + i, col].Style.Fill.BackgroundColor.SetColor(255, 214, 224, 242);
                else
                    sheet.Cells[row + i, col].Style.Fill.BackgroundColor.SetColor(255, 180, 198, 231);
                WriteComment(sheet, res.Answers.Select(e => e.Comment).ToArray()[i], res.User.Fio, row + i, col);
            }
        }

        private void WriteQuestions(ExcelWorksheet sheet, TestResult res, int row, int col)
        {
            for (int i = 0; i < res.Answers.Count(); i++)
            {
                sheet.Cells[row + i, col].Value = res.Test.Questions.ToArray()[i].Question1;
                sheet.Cells[row + i, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                if (i % 2 != 0)
                    sheet.Cells[row + i, col].Style.Fill.BackgroundColor.SetColor(255, 159, 183, 225);
                else
                    sheet.Cells[row + i, col].Style.Fill.BackgroundColor.SetColor(255, 121, 154, 213);

                WriteComment(sheet, res.Test.Questions.Select(e => e.Comment).ToArray()[i], res.User.Fio, row + i, col);
            }
        }

        private void WriteComment(ExcelWorksheet sheet, string str, string author, int row, int col)
        {
            if (sheet.Cells[row, col].Comment != null || string.IsNullOrWhiteSpace(str))
                return;
            sheet.Cells[row, col].AddComment(str, author);
            sheet.Cells[row, col].Comment.Visible = false;
        }

        private void WriteNamesColumns(ExcelWorksheet sheet, string[] names, int rowoffset)
        {
            rowoffset++;
            for (int i = 0; i < names.Length; i++)
            {
                sheet.Cells[rowoffset + i, 1].Value = names[i];
                sheet.Cells[rowoffset + i, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[rowoffset + i, 1].Style.Fill.BackgroundColor.SetColor(255, 242, 160, 110);
            }
        }
    }
}