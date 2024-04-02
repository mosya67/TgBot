using Database.Db;
using Domain.Model;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Database.GetFunctions
{
    public class GetTest : IGetCommand<Task<Test>, ushort>
    {
        readonly Context context;

        public GetTest(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Test> Get(ushort testId)
        {
            return await context.Tests.Include(p => p.Questions).SingleOrDefaultAsync(p => p.Id == testId);
        }
    }
}
