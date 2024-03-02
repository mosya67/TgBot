using Database.Database;
using Database.Database.Model;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return context.Users.SingleOrDefault(e => e.TgId == id);
        }
    }
}
