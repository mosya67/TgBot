using Database.Database.Model;
using Database.Database;
using Domain;
using System;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Database.AddFunctions
{
    public class AddTestResultInDb : IWriteCommand<ImmutableList<ValidationResult>, ResultTestDTO>
    {
        readonly IGetCommand<Test, long> getTest;
        readonly IGetCommand<int> countTestResults;
        readonly IGetCommand<User, long> getUser;
        readonly IWriteCommand<TestResult> saveTestResult;

        public AddTestResultInDb(IGetCommand<Test, long> getTest, IGetCommand<int> countTestResults, IGetCommand<User, long> getUser, IWriteCommand<TestResult> saveTestResult)
        {
            this.getTest = getTest ?? throw new ArgumentNullException(nameof(getTest));
            this.countTestResults = countTestResults ?? throw new ArgumentNullException(nameof(countTestResults));
            this.getUser = getUser ?? throw new ArgumentNullException(nameof(getUser));
            this.saveTestResult = saveTestResult ?? throw new ArgumentNullException(nameof(saveTestResult));
        }

#warning изменить ImmutableList<ValidationResult> на исключения
        public ImmutableList<ValidationResult> Write(ResultTestDTO dto)
        {
            var test = getTest.Get(dto.TestId);

            var user = getUser.Get(dto.UserId) ?? new User
            {
                TgId = dto.TgId,
                Fio = dto.fio
            };
            var result = new TestResult()
            {
                Date = dto.startDate,
                User = user,
                Test = test,
                Answers = dto.Answers.ToHashSet(),
                Id = countTestResults.Get() + 1,
                Comment = dto.CommentFromTest,
                AdditionalComment = dto.AdditionalCommentForTest,
                Apparat = dto.Device,
                Release = dto.Release,
                UserId = user.UserId,
            };

            saveTestResult.Write(result);

            return null;
        }
    }
}
