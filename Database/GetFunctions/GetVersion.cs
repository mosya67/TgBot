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
    public class GetVersion : IGetCommand<Task<TestVersion>, uint>
    {
        readonly Context context;

        public GetVersion(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<TestVersion> Get(uint id)
        {
            return await context.TestVersions.AsNoTracking().Include(e => e.Questions).AsNoTracking().SingleOrDefaultAsync(e => e.Id == id);
        }
    }
}
