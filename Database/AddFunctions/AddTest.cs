using Database.Db;
using Domain;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Database.AddFunctions
{
    public class AddTest : IWriteCommand<Task, Test>
    {
        readonly Context context;

        public AddTest(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Write(Test test)
        {
            uint Versionid = 1;
            var testId = 1;
            if (await context.Tests.AsNoTracking().CountAsync() > 0)
            {
                var a = await context.Tests.AsNoTracking().OrderBy(e => e.Id).LastAsync();
                testId = a.Id + 1;
            }
            await context.Tests.AddAsync(test);
            await context.SaveChangesAsync();
            if (await context.TestVersions.AsNoTracking().CountAsync() > 0)
            {
                var a = await context.TestVersions.AsNoTracking().OrderBy(e => e.Id).LastAsync();
                Versionid = a.Id + 1;
            }
            var ver = new TestVersion
            {
                Id = Versionid,
                DateCreated = DateTime.Now,
                Questions = test.Questions,
                TestId = (ushort)testId,
            };
            test.TestVersionId = (uint)testId;
            await context.TestVersions.AddAsync(ver);
            await context.SaveChangesAsync();
        }
    }
}
