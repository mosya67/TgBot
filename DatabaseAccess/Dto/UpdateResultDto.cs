using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class UpdateResultDto
    {
        public int Id;
        public IList<Answer> answers;
        public bool isStopped;
        public sbyte? PausedQuestionNumber;
    }
}
