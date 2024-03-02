using DatabaseServices;
using Domain;
using StatusGeneric;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExcelServices
{
    public class ExcelGenerator : IExcelGenerator<Task<FileDto>, DatesForExcelDTO>
    {
#warning изменить DbServices
        private readonly DbServices db;

        public ExcelGenerator() {}

        public async Task<FileDto> WriteResultsAsync(DatesForExcelDTO dto)
        {

            FileDto file = new FileDto();
            string fileName = $"{dto.fdate?.ToShortDateString()}__{dto.ldate?.ToShortDateString()}.xlsx";
            var reportExcel = new WriteDataInExcel().Generate(db.GetData(dto.fdate, dto.ldate));
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
