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
    public class GetTestResult : IGetCommand<Task<TestResult>, int>
    {
        readonly Context context;

        public GetTestResult(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<TestResult> Get(int id)
        {
            return await context.TestResults.Include(e => e.Answers).Include(e => e.Test).AsNoTracking().SingleOrDefaultAsync(e => e.Id == id);
        }
    }
}
