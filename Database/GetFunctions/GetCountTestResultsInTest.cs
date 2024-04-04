using Database.Db;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.GetFunctions
{
    public class GetCountTestResultsInTest : IGetCommand<Task<int>, ushort>
    {
        readonly Context context;

        public GetCountTestResultsInTest(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> Get(ushort parameter)
        {
            return await context.TestResults.AsNoTracking().Include(e => e.Test).AsNoTracking().Where(e => e.Test.Id == parameter).CountAsync();
        }
    }
}
