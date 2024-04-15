using Newtonsoft.Json;

#nullable disable

namespace Domain.Model
{
    public partial class Question
    {
        [JsonIgnore]
        public ushort Id { get; set; }
        public string question { get; set; }
        public string ExpectedResult { get; set; }
        public string Comment { get; set; }

        [JsonIgnore]
        public Test Test { get; set; }
    }
}
