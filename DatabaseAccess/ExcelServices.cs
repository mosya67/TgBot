using DatabaseServices;
using StatusGeneric;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExcelServices
{
    public class ExServices
    {
        private readonly string path = "";
        private readonly DbServices db = new();


        public ExServices(string path)
        {
            this.path = path;
        }

        public ExServices() {}

        public async Task<FileDto> WriteResultsAsync(DateTime? fdate, DateTime? ldate)
        {
            FileDto file = new FileDto();
            string fileName = $"{fdate?.ToShortDateString()}__{ldate?.ToShortDateString()}.xlsx";
            var reportExcel = new ExcelGenerator().Generate(db.GetData(fdate, ldate));
            if (reportExcel.HasErrors)
            {
                file.Errors = reportExcel.Errors.Select(e => e.ErrorResult.ErrorMessage).ToList();
                return file;
            }

            await File.WriteAllBytesAsync(path + fileName, reportExcel.Result);
            file.PathName = path + fileName;
            return file;
        }
    }
}
