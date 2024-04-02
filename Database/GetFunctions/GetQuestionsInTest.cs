using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.Db;
using Domain.Model;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Database.GetFunctions
{
    public class GetQuestionsInTest : IGetCommand<Task<IList<Question>>, long>
    {
        readonly Context context;

        public GetQuestionsInTest(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IList<Question>> Get(long id)
        {
            var test = await context.Tests.AsNoTracking().Include(p => p.Questions).SingleOrDefaultAsync(x => x.Id == id);
            return test.Questions.ToList();
        }
    }
}
