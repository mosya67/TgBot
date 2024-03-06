using Database.Db;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Database.GetFunctions
{
    public class GetCountQuestionsInTest : IGetCommand<sbyte, long>
    {
        readonly Context context;

        public GetCountQuestionsInTest(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public sbyte Get(long testId)
        {
            return (sbyte)context.Questions.AsNoTracking().Where(p => p.Test.Id == testId).Count();
        }
    }
}
