using System;
using System.Collections.Generic;

#nullable disable

namespace Database
{
    public partial class Test
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }

        public HashSet<TestResult> TestResult { get; set; }
        public HashSet<Question> Questions { get; set; }
    }
}
