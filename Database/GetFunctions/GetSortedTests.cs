﻿using Database.Db;
using Domain.Model;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.GetFunctions
{
    public class GetSortedTests : IGetCommand<Task<IList<Test>>>
    {
        readonly Context context;

        public GetSortedTests(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IList<Test>> Get()
        {
            return await context.Tests.AsNoTracking().OrderByDescending(p => p.Date.Date).ToListAsync();
        }
    }
}
