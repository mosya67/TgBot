using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace Database
{
    public partial class TestResult
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; }
        public string AdditionalComment { get; set; }
        public string Apparat { get; set; }
        public string Release { get; set; }
        public int UserId { get; set; }

        public User User { get; set; }
        public Test Test { get; set; }
        public HashSet<Answer> Answers { get; set; }
    }
}
