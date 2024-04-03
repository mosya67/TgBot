using Database.Db;
using Domain;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.GetFunctions
{
    public class GetLastResult : IGetCommand<Task<TestResult>, ushort>
    {
        readonly Context context;

        public GetLastResult(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<TestResult> Get(ushort testId)
        {
            return await context.TestResults.Where(e => !e.IsPaused && e.Test.Id == testId).OrderByDescending(e => e.Date).FirstOrDefaultAsync();
        }
    }
}
