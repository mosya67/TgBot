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
    public class GetTestPage : IGetCommand<Task<IEnumerable<Test>>, PageDto>
    {
        readonly Context context;

        public GetTestPage(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Test>> Get(PageDto dto)
        {
            return await context.Tests.Skip(dto.startPage * dto.countElementsInPage).Take(dto.countElementsInPage).ToListAsync();
        }
    }
}
