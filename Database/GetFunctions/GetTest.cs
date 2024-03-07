using Database.Db;
using Domain.Model;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Database.GetFunctions
{
    public class GetTest : IGetCommand<Test, ushort>
    {
        readonly Context context;

        public GetTest(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Test Get(ushort testId)
        {
            return context.Tests.Include(p => p.Questions).SingleOrDefault(p => p.Id == testId);
        }
    }
}
