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
    public class CheckAnswersCount : IGetCommand<int>
    {
        readonly Context context;

        public CheckAnswersCount(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public int Get()
        {
            return context.Answers.AsNoTracking().Count();
        }
    }
}
