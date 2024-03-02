using Database.Database;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.GetFunctions
{
    public class GetCountQuestionsInTest : IGetCommand<int, long>
    {
        readonly Context context;

        public GetCountQuestionsInTest(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public int Get(long testId)
        {
            return context.Questions.AsNoTracking().Where(p => p.Test.Id == testId).Count();
        }
    }
}
