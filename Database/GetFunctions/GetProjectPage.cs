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
    public class GetProjectPage : IGetCommand<Task<IEnumerable<Project>>, PageDto>
    {
        readonly Context context;

        public GetProjectPage(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Project>> Get(PageDto dto)
        {
            return await context.Projects.Skip(dto.startPage * dto.countElementsInPage).Take(dto.countElementsInPage + 1).ToListAsync();
        }
    }
}
