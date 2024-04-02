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
    public class GetStoppedTests : IGetCommand<Task<IEnumerable<TestResult>>, long>
    {
        readonly Context context;

        public GetStoppedTests(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<TestResult>> Get(long userId)
        {
            return await context.TestResults.Include(e => e.Test).Include(e => e.User).Include(e => e.Answers).AsNoTracking().Where(e => e.IsPaused && e.User.TgId == userId).ToListAsync();
        }
    }
}
