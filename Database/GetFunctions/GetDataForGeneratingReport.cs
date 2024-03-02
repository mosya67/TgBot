using Database.Database;
using Database.Database.Model;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Database.GetFunctions
{
    public class GetDataForGeneratingReport : IGetCommand<IEnumerable<TestResult>, DatesForExcelDTO>
    {
        readonly Context context;

        public GetDataForGeneratingReport(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<TestResult> Get(DatesForExcelDTO dates)
        {
            var results = context.TestResults
                    .Include(p => p.Answers)
                    .Include(e => e.User)
                    .Include(e => e.Test)
                    .ThenInclude(p => p.Questions)
                    .AsNoTracking();

            if (dates.fdate == null && dates.ldate != null)
            {
                results.Where(p => p.Date.Date <= dates.ldate.Value.Date);
            }
            else if (dates.fdate != null && dates.ldate == null)
            {
                results.Where(p => p.Date.Date >= dates.fdate.Value.Date);
            }
            else if (dates.fdate != null && dates.ldate != null)
            {
                results.Where(e => e.Date.Date >= dates.fdate.Value.Date && e.Date.Date <= dates.ldate.Value.Date);
            }

            return results.AsEnumerable();
        }
    }
}
