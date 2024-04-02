using Database.Db;
using Domain;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.AddFunctions
{
    public class AddNewUser : IWriteCommand<Task, string>
    {
        readonly Context context;

        public AddNewUser(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Write(string nickname)
        {
            await context.Users.AddAsync(new User() { Fio = nickname, TgId = new Random().Next() });
            await context.SaveChangesAsync();
        }
    }
}
