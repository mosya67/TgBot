﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Model
{
    public partial class Test
    {
        public ushort Id { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }

        public IList<TestResult> TestResult { get; set; }
        public IList<Question> Questions { get; set; }
    }
}
