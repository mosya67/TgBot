using Database.Database;
using Database.Database.Model;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.GetFunctions
{
    public class GetTest : IGetCommand<Test, long>
    {
        readonly Context context;

        public GetTest(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Test Get(long testId)
        {
            return context.Tests.AsNoTracking().SingleOrDefault(p => p.Id == testId);
        }
    }
}
