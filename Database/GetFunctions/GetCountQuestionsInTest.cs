using Database.Db;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.GetFunctions
{
    public class GetCountQuestionsInTest : IGetCommand<Task<sbyte>, long>
    {
        readonly Context context;

        public GetCountQuestionsInTest(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<sbyte> Get(long testId)
        {
            return  (sbyte)await context.Questions.AsNoTracking().Where(p => p.Test.Id == testId).CountAsync();
        }
    }
}
