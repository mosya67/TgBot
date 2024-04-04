using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class ReportExcelDTO
    {
        public DateTime? fdate;
        public DateTime? ldate;
        public uint TestVersion;
        public sbyte variant;
        public ushort TestId;
    }
}
