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
        readonly IGetCommand<Task<IList<TestResult>>, ReportExcelDTO> getData;
        readonly IGetCommand<Task<TestVersion>, uint> getTestVersion;

        public ExcelGenerator(IGetCommand<Task<IList<TestResult>>, ReportExcelDTO> getData, IGetCommand<Task<TestVersion>, uint> getTestVersion)
        {
            this.getData = getData ?? throw new ArgumentNullException(nameof(getData));
            this.getTestVersion = getTestVersion ?? throw new ArgumentNullException(nameof(getTestVersion));
        }

        public async Task<FileDto> WriteResultsAsync(ReportExcelDTO dto)
        {
            IStatusGeneric<byte[]> reportExcel = null;
            List<(TestResult, TestVersion)> data = new();
            FileDto file = new FileDto();
            var testResults = await getData.Get(dto);
            testResults = testResults.OrderByDescending(x => x.Date).ToList();
            for (int i = 0; i < testResults.Count(); i++)
            {
                data.Add((testResults[i], await getTestVersion.Get(testResults[i].TestVersionId)));
            }
            reportExcel = await new WriteDataInExcel().Generate(data);
            if (reportExcel.HasErrors)
            {
                file.Errors = reportExcel.Errors.Select(e => e.ErrorResult.ErrorMessage).ToList();
                return file;
            }
            string fileName = $"{new Random().Next()}.xlsx";
            await File.WriteAllBytesAsync(fileName, reportExcel.Result);
            file.PathName = fileName;
            return file;
        }
    }
}
