using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class TestVersion
    {
        public uint Id { get; set; }
        public DateTime DateCreated { get; set; }
        public ushort TestId { get; set; }
        public IList<Question> Questions { get; set; }
    }
}
