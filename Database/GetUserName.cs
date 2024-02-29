using Database.Database;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class GetUserName : IGetCommand<string, long>
    {
        readonly Context context;

        public GetUserName(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string Get(long i)
        {
            return context.Users.SingleOrDefault(e => e.TgId == Id)?.Fio;
        }
    }
}
