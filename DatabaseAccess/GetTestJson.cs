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

            string json = JsonConvert.SerializeObject(test, settings);
            var path = "C:\\Users\\admin\\source\\repos\\TgBot\\TgBot\\bin\\Debug\\net5.0\\" + new Random().Next().ToString() + ".json";
            await File.WriteAllTextAsync(path, json);

            return path;
        }
    }
}
