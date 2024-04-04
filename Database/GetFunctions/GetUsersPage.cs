using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Domain;
using Domain.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.Db;

namespace Database.GetFunctions
{
    public class GetUsersPage : IGetCommand<Task<IEnumerable<User>>, PageDto>
    {
        readonly Context context;

        public GetUsersPage(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<User>> Get(PageDto dto)
        {
            return await context.Users.Skip(dto.startPage * dto.countElementsInPage).Take(dto.countElementsInPage + 1).ToListAsync();
        }
    }
}
