using Domain.Model;
using Domain;
using Domain.Dto;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ExcelServices
{
    public class ExcelGenerator : IExcelGenerator<Task<FileDto>, DatesForExcelDTO>
    {
        readonly IGetCommand<IList<TestResult>, DatesForExcelDTO> getData;

        public ExcelGenerator(IGetCommand<IList<TestResult>, DatesForExcelDTO> getData)
        {
            this.getData = getData ?? throw new ArgumentNullException(nameof(getData));
        }

        public async Task<FileDto> WriteResultsAsync(DatesForExcelDTO dto)
        {

            FileDto file = new FileDto();
            string fileName = $"{dto.fdate?.ToShortDateString()}__{dto.ldate?.ToShortDateString()}.xlsx";
            var reportExcel = new WriteDataInExcel().Generate(getData.Get(dto));
            if (reportExcel.HasErrors)
            {
                file.Errors = reportExcel.Errors.Select(e => e.ErrorResult.ErrorMessage).ToList();
                return file;
            }

            await File.WriteAllBytesAsync(fileName, reportExcel.Result);
            file.PathName = fileName;
            return file;
        }
    }
}
