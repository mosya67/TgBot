using Domain.Model;
using Domain;
using Domain.Dto;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using StatusGeneric;

namespace ExcelServices
{
    public class ExcelGenerator : IExcelGenerator<Task<FileDto>, ReportExcelDTO>
    {
        readonly IGetCommand<Task<IList<TestResult>>, ReportExcelDTO> getResults;

        public ExcelGenerator(IGetCommand<Task<IList<TestResult>>, ReportExcelDTO> getResults)
        {
            this.getResults = getResults ?? throw new ArgumentNullException(nameof(getResults));
        }

        public async Task<FileDto> WriteResultsAsync(ReportExcelDTO dto)
        {
            IStatusGeneric<byte[]> reportExcel = null;
            FileDto file = new FileDto();
            IList<TestResult> results = new List<TestResult>();

            results = await getResults.Get(dto);

            if (results.Count() == 0) return new FileDto { Errors = new List<string> { "результаты не найдены" } };

            reportExcel = new WriteDataInExcel().Generate(results);
            if (reportExcel.HasErrors)
            {
                file.Errors = reportExcel.Errors.Select(e => e.ErrorResult.ErrorMessage).ToList();
                return file;
            }
            string fileName = $"{results[0].Test.Name} {DateTime.Now.ToShortDateString()}.xlsx";
            await File.WriteAllBytesAsync(fileName, reportExcel.Result);
            file.PathName = fileName;
            return file;
        }
    }
}
