using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;
using Database.Db;

namespace Database.GetFunctions
{
    public class GetUserName : IGetCommand<Task<string>, long>
    {
        readonly Context context;

        public GetUserName(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<string> Get(long Id)
        {
            var user = await context.Users.AsNoTracking().SingleOrDefaultAsync(e => e.TgId == Id);
            return user?.Fio;
        }
    }
}
