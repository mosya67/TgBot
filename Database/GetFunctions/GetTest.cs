using Database.Db;
using Domain.Model;
using Domain;
using Domain.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
