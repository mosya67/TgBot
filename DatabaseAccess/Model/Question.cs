﻿
#nullable disable

namespace Domain.Model
{
    public partial class Question
    {
        public ushort Id { get; set; }
        public string Question1 { get; set; }
        public string Comment { get; set; }

        public Test Test { get; set; }
    }
}
