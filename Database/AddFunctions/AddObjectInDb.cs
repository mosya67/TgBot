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
    public class AddObjectInDb<TIn> : IWriteCommand<TIn>
    {
        readonly Context context;

        public AddObjectInDb(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Write(TIn _object)
        {
            context.Add(_object);

            context.SaveChanges();
        }
    }
}
