using Database.Db;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.GetFunctions
{
    public class GetCountUsers : IGetCommand<Task<int>>
    {
        readonly Context context;

        public GetCountUsers(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> Get()
        {
            return await context.Users.AsNoTracking().CountAsync();
        }
    }
}
