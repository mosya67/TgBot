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
    public class AddNewDevice : IWriteCommand<Task, string>
    {
        readonly Context context;

        public AddNewDevice(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Write(string nickname)
        {
            await context.Devices.AddAsync(new Device() { Name = nickname });
            await context.SaveChangesAsync();
        }
    }
}
