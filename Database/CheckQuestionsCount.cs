using Database.Database;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class CheckQuestionsCount : IGetCommand<int, long>
    {
        readonly Context context;

        public CheckQuestionsCount(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public int Get(long testId)
        {
            return context.Questions.AsNoTracking().Where(p => p.Test.Id == testId).Count();
        }
    }
}
