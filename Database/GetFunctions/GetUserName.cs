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
    public class GetUserName : IGetCommand<string, long>
    {
        readonly Context context;

        public GetUserName(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string Get(long Id)
        {
            return context.Users.AsNoTracking().SingleOrDefault(e => e.TgId == Id)?.Fio;
        }
    }
}
