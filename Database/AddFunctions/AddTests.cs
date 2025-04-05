using Domain;
using Domain.Model;
using System;
using System.Collections.Generic;

namespace Database.AddFunctions
{
    public class AddTests : IWriteCommand<List<Test>>
    {
        public void Write(List<Test> parameter)
        {
            throw new NotImplementedException();
        }
    }
}
