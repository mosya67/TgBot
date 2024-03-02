using ExcelServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public interface IExcelGenerator<TOut, TIn>
    {
        public TOut WriteResultsAsync(TIn parameter);
    }
}
