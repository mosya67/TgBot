using Database.Db;
using Domain;
using Domain.Dto;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.GetFunctions
{
    public class GetTestPage : IGetCommand<Task<IEnumerable<Test>>, TestPageDto>
    {
        readonly Context context;

        public GetTestPage(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Test>> Get(TestPageDto dto)
        {
            return await context.Tests.AsNoTracking().Include(e => e.Project).AsNoTracking().Where(e => e.Project.Id == dto.projectId).Skip(dto.pageSet.startPage * dto.pageSet.countElementsInPage).Take(dto.pageSet.countElementsInPage + 1).ToListAsync();
        }
    }
}
