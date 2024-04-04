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
        readonly IGetCommand<Task<IList<TestVersion>>, ushort> getVersions;
        readonly IGetCommand<Task<IList<TestResult>>, ReportExcelDTO> getResults;
        readonly IGetCommand<Task<TestVersion>, uint> getVersion;

        public ExcelGenerator(IGetCommand<Task<IList<TestVersion>>, ushort> getVersions, IGetCommand<Task<IList<TestResult>>, ReportExcelDTO> getResults, IGetCommand<Task<TestVersion>, uint> getVersion)
        {
            this.getVersions = getVersions ?? throw new ArgumentNullException(nameof(getVersions));
            this.getResults = getResults ?? throw new ArgumentNullException(nameof(getResults));
            this.getVersion = getVersion ?? throw new ArgumentNullException(nameof(getVersion));
        }

        public async Task<FileDto> WriteResultsAsync(ReportExcelDTO dto)
        {
            IStatusGeneric<byte[]> reportExcel = null;
            FileDto file = new FileDto();
            IList<IList<TestResult>> results = new List<IList<TestResult>>();
            IList<TestVersion> testVersions = new List<TestVersion>();

            if (dto.variant == 1)
            {
                results.Add(await getResults.Get(dto));
                testVersions.Add(await getVersion.Get(results[0][0].TestVersionId));
            }
            else
            {
                var res = await getResults.Get(dto);
                List<uint> versId = new List<uint>();
                for (int i = 0; i < res.Count(); i++)
                {
                    var ver = await getVersion.Get(res[i].TestVersionId);
                    if (!versId.Contains(ver.Id))
                    {
                        testVersions.Add(ver);
                        versId.Add(ver.Id);
                    }
                }

                for (int i = 0; i < testVersions.Count(); i++)
                {
                    results.Add(res.Where(e => e.TestVersionId == testVersions[i].Id).ToList());
                }
            }

            if (results.Count() == 0) return new FileDto { Errors = new List<string> { "результаты не найдены" } };

            reportExcel = await new WriteDataInExcel().Generate(results, testVersions);
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
