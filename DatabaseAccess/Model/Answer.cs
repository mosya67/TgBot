using System;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Domain.Model
{
    public partial class Answer
    {
        public int Id { get; set; }
        public string Result { get; set; }

        [MaxLength(128, ErrorMessage = "превышена длинна комментария")]
        public string Comment { get; set; }

        public TestResult TestResult { get; set; }
    }
}
