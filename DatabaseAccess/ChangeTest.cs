using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Model;
using Newtonsoft.Json;

namespace Domain
{
    public class ChangeTest : IWriteCommand<Task, Stream>
    {
        readonly IWriteCommand<Task, Test> updateTest;

        public ChangeTest(IWriteCommand<Task, Test> updateTest)
        {
            this.updateTest = updateTest ?? throw new ArgumentNullException(nameof(updateTest));
        }

        public async Task Write(Stream stream)
        {
            stream.Position = 0;
            using (var reader = new StreamReader(stream))
            {
                string jsonContent = await reader.ReadToEndAsync();

                var test = JsonConvert.DeserializeObject<Test>(jsonContent);

                await updateTest.Write(test);
            }
        }
    }
}
