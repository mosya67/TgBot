using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace Domain.Model
{
    public partial class TestResult
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; }
        public string AdditionalComment { get; set; }
        public string Apparat { get; set; }
        public string Version { get; set; }
        public int UserId { get; set; }
        public byte? PausedQuestionNumber { get; set; }
        public bool IsPaused { get; set; }

        public User User { get; set; }
        public Test Test { get; set; }
        public IList<Answer> Answers { get; set; }
    }
}
