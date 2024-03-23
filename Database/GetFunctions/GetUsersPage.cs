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
    public class GetUsersPage : IGetCommand<IEnumerable<User>, UserPageDto>
    {
        readonly Context context;

        public GetUsersPage(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<User> Get(UserPageDto dto)
        {
            return context.Users.Skip(dto.startPage * dto.countElementsInPage).Take(dto.countElementsInPage);
        }
    }
}
