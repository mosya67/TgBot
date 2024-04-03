using Database.Db;
using Domain.Model;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Database.AddFunctions
{
    public class UpdateTest : IWriteCommand<Task, Test>
    {
        IGetCommand<Task<Test>, ushort> getTest;
        readonly Context context;

        public UpdateTest(IGetCommand<Task<Test>, ushort> getTest, Context context)
        {
            this.getTest = getTest ?? throw new ArgumentNullException(nameof(getTest));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Write(Test newTest)
        {
            var oldTest = await getTest.Get(newTest.Id);

            var lastQuest = await context.Questions.AsNoTracking().OrderBy(e => e.Id).LastOrDefaultAsync();
            var lastVers = await context.TestVersions.AsNoTracking().OrderBy(e => e.Id).LastOrDefaultAsync();
            for (ushort i = 0; i < newTest.Questions.Count(); i++)
            {
                newTest.Questions[i] = new Question
                {
                    Comment = newTest.Questions[i].Comment,
                    question = newTest.Questions[i].question,
                    Id = (ushort)(lastQuest.Id + i + 1),
                };
            }

            await context.TestVersions.AddAsync(new TestVersion
            {
                Id = (ushort)(lastVers.Id + 1),
                Questions = newTest.Questions,
                DateCreated = DateTime.Now,
                TestId = newTest.Id,
            });
            await context.SaveChangesAsync();
            oldTest.TestVersionId = (ushort)(lastVers.Id + 1);
            oldTest.Comment = newTest.Comment;
            oldTest.Name = newTest.Name;
            oldTest.Date = newTest.Date;

            oldTest.Questions = newTest.Questions;
            await context.SaveChangesAsync();
        }
    }
}
