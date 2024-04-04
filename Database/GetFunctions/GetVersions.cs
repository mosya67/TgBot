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
    public class GetVersions : IGetCommand<Task<IList<TestVersion>>, ushort>
    {
        readonly Context context;

        public GetVersions(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IList<TestVersion>> Get(ushort testId)
        {
            return await context.TestVersions.AsNoTracking().Include(e => e.Questions).AsNoTracking()
                .Where(e => e.TestId == testId).OrderBy(e => e.Id).ToListAsync();
        }
    }
}
