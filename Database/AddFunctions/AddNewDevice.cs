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
    public class AddNewDevice : IWriteCommand<string>
    {
        readonly Context context;

        public AddNewDevice(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Write(string nickname)
        {
            context.Devices.Add(new Device() { Name = nickname});
            context.SaveChanges();
        }
    }
}
