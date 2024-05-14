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
    public class GetStoppedTestsPage : IGetCommand<Task<IEnumerable<TestResult>>, StoppedTestResultPageDto>
    {
        readonly Context context;

        public GetStoppedTestsPage(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<TestResult>> Get(StoppedTestResultPageDto dto)
        {
            return await context.TestResults.AsNoTracking().OrderByDescending(e => e.Date).Include(e => e.Test).AsNoTracking()
                .Include(e => e.User).AsNoTracking()
                .Where(e => e.User.TgId == dto.userId && e.Test.Id == dto.testId && e.IsPaused)
                .Skip(dto.pageSet.startPage * dto.pageSet.countElementsInPage).Take(dto.pageSet.countElementsInPage + 1).ToListAsync();
        }
    }
}
