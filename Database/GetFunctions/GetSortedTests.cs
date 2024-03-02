﻿using Database.Database;
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
    public class GetSortedTests : IGetCommand<List<Test>>
    {
        readonly Context context;

        public GetSortedTests(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public List<Test> Get()
        {
            return context.Tests.AsNoTracking().OrderByDescending(p => p.Date.Date).ToList();
        }
    }
}