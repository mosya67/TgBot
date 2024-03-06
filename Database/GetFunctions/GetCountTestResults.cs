using Database.Db;
using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.GetFunctions
{
    public class GetCountTestResults : IGetCommand<ushort>
    {
        readonly Context context;

        public GetCountTestResults(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ushort Get()
        {
            return (ushort)context.TestResults.AsNoTracking().Count();
        }
    }
}
