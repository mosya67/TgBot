using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Dto
{
    public class AnswerDto
    {
        public long testId { get; set; }

        [MaxLength(32, ErrorMessage = "длинна имени не может быть больше 32")]
        [Required(ErrorMessage = "fio не может быть null")]
        public string fio { get; set; }
        public long TgId { get; set; }
        public string answer { get; set; }
        public DateTime startDate { get; set; }

        [MaxLength(128, ErrorMessage = "превышена длинна комментария")]
        public string comment { get; set; }
        public List<Answer> Answers { get; set; }
        [MaxLength(128, ErrorMessage = "превышена длинна комментария")]
        public string CommentFromTest { get; set; }
        [MaxLength(128, ErrorMessage = "превышена длинна доп комментария")]
        public string AdditionalCommentForTest { get; set; }
        public DateTime? ldate { get; set; }
        public DateTime? fdate { get; set; }
        public string Device { get; set; }
        public string Release { get; set; }

        public AnswerDto() { }
    }
}
