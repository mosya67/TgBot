using Domain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class ResultTestDto
    {
        public IList<ValidationResult> Errors;
        public IList<Answer> Answers;
        public long TestId;
        public long UserId;
    }
}
