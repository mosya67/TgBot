using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class Project
    {
        public ushort Id { get; set; }
        public string Name { get; set; }
        public IList<Test> Tests{ get; set; }
    }
}
