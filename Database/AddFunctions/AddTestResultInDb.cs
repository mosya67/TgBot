using Domain;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic;
using Domain.Dto;
using Domain.Model;
using Database.GetFunctions;
using System.Threading.Tasks;
using Database.Db;

namespace Database.AddFunctions
{
    public class AddTestResultInDb : IWriteCommand<Task<IReadOnlyList<ValidationResult>>, ResultTestDto>
    {
        readonly IGetCommand<Task<int>> countTestResults;
        readonly IWriteCommand<Task, UpdateResultDto> updateTestResult;
        readonly IGetCommand<Task<Test>, ushort> getTest;
        readonly IGetCommand<Task<User>, long> getUser;
        readonly IGetCommand<Task<TestResult>, int> getTestResult;
        readonly Context context;

        public AddTestResultInDb(IGetCommand<Task<int>> countTestResults, IWriteCommand<Task, UpdateResultDto> updateTestResult, IGetCommand<Task<Test>, ushort> getTest, IGetCommand<Task<User>, long> getUser, IGetCommand<Task<TestResult>, int> getTestResult, Context context)
        {
            this.countTestResults = countTestResults ?? throw new ArgumentNullException(nameof(countTestResults));
            this.updateTestResult = updateTestResult ?? throw new ArgumentNullException(nameof(updateTestResult));
            this.getTest = getTest ?? throw new ArgumentNullException(nameof(getTest));
            this.getUser = getUser ?? throw new ArgumentNullException(nameof(getUser));
            this.getTestResult = getTestResult ?? throw new ArgumentNullException(nameof(getTestResult));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IReadOnlyList<ValidationResult>> Write(ResultTestDto dto)
        {
            var res = await getTestResult.Get(dto.TestResultId);
            if (res != null)
            {
                await updateTestResult.Write(new UpdateResultDto
                {
                    answers = dto.Answers,
                    isStopped = true,
                    Id = res.Id,
                    PausedQuestionNumber = dto.PausedQuestionNumber,
                });
                return null;
            }

            var user = await getUser.Get(dto.UserId) ?? new User
            {
                Fio = dto.UserName,
                TgId = dto.UserId,
            };
            var test = await getTest.Get((ushort)dto.Test.Id);
            var countResults = await countTestResults.Get();
            var result = new TestResult()
            {
                Date = DateTime.Now,
                User = user,
                Test = test,
                Answers = dto.Answers,
                Id = countResults + 1,
                Comment = dto.CommentFromTest,
                AdditionalComment = dto.AdditionalCommentForTest,
                Apparat = dto.Device,
                Version = dto.Version,
                IsPaused = dto.IsPaused,
                PausedQuestionNumber = dto.PausedQuestionNumber,
                TestVersionId = dto.TestVersionId,
            };

            await context.AddAsync(result);

            await context.SaveChangesAsync();

            return null;
        }
    }
}
