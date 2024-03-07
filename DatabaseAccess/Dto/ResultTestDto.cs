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
        public IList<Answer> Answers;
        public long UserId;
        public string UserName;
        public string CommentFromTest;
        public string AdditionalCommentForTest;
        public string Device;
        public string Release;
        public Test Test;
    }
}
