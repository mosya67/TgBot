using Database.Db;
using Domain;
using Domain.Dto;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.GetFunctions
{
    public class GetLastAnswersOnQuestion : IGetCommand<Task<IList<Answer>>, LastResultDto>
    {
        readonly Context context;

        public GetLastAnswersOnQuestion(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IList<Answer>> Get(LastResultDto dto)
        {
            var tmp = await context.TestResults
                .AsNoTracking()
                .Include(e => e.Answers)
                .AsNoTracking()
                .Include(e => e.Test)
                .AsNoTracking()
                .Where(e => !e.IsPaused && e.Test.Id == dto.testId)
                .OrderByDescending(e => e.Date)
                .Take(dto.resultsCount)
                .ToListAsync();

            if (tmp.Where(e => e.Answers.Count() < dto.QuestNumb).Count() != 0)
                return await Task.Run(() => tmp.Select(e => e.Answers[dto.QuestNumb]).ToList());
            else
                return null;
        }
    }
}
