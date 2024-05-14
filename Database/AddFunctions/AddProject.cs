using Database.Db;
using Domain.Model;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.AddFunctions
{
    public class AddProject : IWriteCommand<Task, Project>
    {
        readonly Context context;

        public AddProject(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Write(Project prj)
        {
            await context.Projects.AddAsync(prj);
            await context.SaveChangesAsync();
        }
    }
}
