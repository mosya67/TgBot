using Database.Db;
using Domain.Model;
using Domain;
using Domain.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.GetFunctions
{
    public class GetDataForGeneratingReport : IGetCommand<Task<IList<TestResult>>, ReportExcelDTO>
    {
        readonly Context context;

        public GetDataForGeneratingReport(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IList<TestResult>> Get(ReportExcelDTO dto)
        {
            IList<TestResult> results = new List<TestResult>();
            var temp = context.TestResults.AsNoTracking()
                        .Include(p => p.Answers).AsNoTracking()
                        .Include(e => e.User).AsNoTracking()
                        .Include(e => e.Test)
                            .ThenInclude(p => p.Questions).AsNoTracking().Where(e => !e.IsPaused);

            switch (dto.variant)
            {
                case 1:
                    results.Add(await temp.OrderByDescending(e => e.Date).FirstOrDefaultAsync());
                    break;
                case 2:
                    results = await temp.OrderByDescending(e => e.Date).Take(3).ToListAsync();
                    break;
                case 3:
                    results = await temp.ToListAsync();
                    break;
                case 4:
                    if (dto.fdate == null && dto.ldate != null)
                    {
                        results = await temp.Where(p => p.Date.Date <= dto.ldate.Value.Date).ToListAsync();
                    }
                    else if (dto.fdate != null && dto.ldate == null)
                    {
                        results = await temp.Where(p => p.Date.Date >= dto.fdate.Value.Date).ToListAsync();
                    }
                    else if (dto.fdate != null && dto.ldate != null)
                    {
                        results = await temp.Where(e => e.Date.Date >= dto.fdate.Value.Date && e.Date.Date <= dto.ldate.Value.Date).ToListAsync();
                    }
                    else
                    {
                        results = await temp.ToListAsync();
                    }
                    break;
            }

            return results;
        }
    }
}
