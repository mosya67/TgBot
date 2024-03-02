using Database.Database;
using Database.Database.Model;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.AddFunctions
{
#warning класс ResetName переделать на SetName
    public class ResetName : IWriteCommand<bool, ResetNameDTO>
    {
        readonly Context context;
        readonly IGetCommand<User, long> getUser;

        public ResetName(Context context, IGetCommand<User, long> getUser)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.getUser = getUser ?? throw new ArgumentNullException(nameof(getUser));
        }

        public bool Write(ResetNameDTO parameter)
        {
            var user = getUser.Get(parameter.Id);
            if (user != null)
            {
                user.Fio = parameter.Name;
                context.Update(user);
                context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
