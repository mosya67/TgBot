﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.Database;
using Database.Database.Model;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Database.GetFunctions
{
    public class GetQuestionsInTest : IGetCommand<IEnumerable<Question>, long>
    {
        readonly Context context;

        public GetQuestionsInTest(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<Question> Get(long id)
        {
            return context.Tests.AsNoTracking().Include(p => p.Questions).SingleOrDefault(x => x.Id == id).Questions.ToList();
        }
    }
}