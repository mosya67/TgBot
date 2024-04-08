using Domain.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class GetTestJson : IGetCommand<Task<string>, ushort>
    {
        readonly IGetCommand<Task<Test>, ushort> getTest;

        public GetTestJson(IGetCommand<Task<Test>, ushort> getTest)
        {
            this.getTest = getTest ?? throw new ArgumentNullException(nameof(getTest));
        }

        public async Task<string> Get(ushort testId)
        {
            var test = await getTest.Get(testId);
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
            var now = DateTime.Now;
            string json = JsonConvert.SerializeObject(test, settings);
            var path = now.Hour + " " + now.Minute + " " + +now.Second + " " + now.Millisecond + " " + now.ToShortDateString() + ".json";
            await File.WriteAllTextAsync(path, json);

            return path;
        }
    }
}
