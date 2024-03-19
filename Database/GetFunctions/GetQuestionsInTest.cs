using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.Db;
using Domain.Model;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Database.GetFunctions
{
    public class GetQuestionsInTest : IGetCommand<IList<Question>, long>
    {
        readonly Context context;

        public GetQuestionsInTest(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IList<Question> Get(long id)
        {
#warning подумать над получением не вопросов из теста, а самих вопросов по TestId
            return context.Tests.AsNoTracking().Include(p => p.Questions).SingleOrDefault(x => x.Id == id).Questions.ToList();
        }
    }
}
