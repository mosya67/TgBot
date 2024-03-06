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
    public class GetUser : IGetCommand<User, long>
    {
        readonly Context context;

        public GetUser(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public User Get(long id)
        {
            return context.Users.AsNoTracking().SingleOrDefault(e => e.TgId == id);
        }
    }
}
