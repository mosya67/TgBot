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

namespace Database.AddFunctions
{
    public class UpdateResult : IWriteCommand<Task, UpdateResultDto>
    {
        readonly Context context;

        public UpdateResult(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Write(UpdateResultDto dto)
        {
            var result = await context.TestResults.SingleOrDefaultAsync(e => e.Id == dto.Id);
            result.Answers = dto.answers;
            result.Date = DateTime.Now;
            result.IsPaused = dto.isStopped;
            result.PausedQuestionNumber = dto.PausedQuestionNumber;
            context.Update(result);
            await context.SaveChangesAsync();
        }
    }
}
