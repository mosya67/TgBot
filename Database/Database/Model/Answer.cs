using System;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Database.Database.Model
{
    public partial class Answer
    {
        public int Id { get; set; }
        [MaxLength(64, ErrorMessage = "длинна ответа слишком большая")]
        [Required(ErrorMessage = "ответ не может быть пустым")]
        public string Result { get; set; }

        [MaxLength(128, ErrorMessage = "превышена длинна комментария")]
        public string Comment { get; set; }
        //public string FileName { get; set; }

        public TestResult TestResult { get; set; }
    }
}
