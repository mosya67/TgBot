using System.Collections;
using System.Collections.Generic;

namespace Domain.Model
{
    public class User
    {
        public int UserId { get; set; }
        public long TgId { get; set; }
        public string Fio { get; set; }
    }
}