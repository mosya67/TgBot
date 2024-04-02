using Database.Db;
using Domain.Model;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.AddFunctions
{
    public class UpdateTest : IWriteCommand<Task, Test>
    {
        readonly Context context;

        public UpdateTest(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Write(Test test)
        {
            context.Tests.Update(test);
            await context.SaveChangesAsync();
        }
    }
}
