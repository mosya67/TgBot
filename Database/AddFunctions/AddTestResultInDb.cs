using Domain;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic;
using Domain.Dto;
using Domain.Model;
using Database.GetFunctions;

namespace Database.AddFunctions
{
    public class AddTestResultInDb : IWriteCommand<IReadOnlyList<ValidationResult>, ResultTestDto>
    {
        readonly IGetCommand<int> countTestResults;
        readonly IWriteCommand<TestResult> saveTestResult;

        public AddTestResultInDb(IGetCommand<int> countTestResults, IWriteCommand<TestResult> saveTestResult)
        {
            this.countTestResults = countTestResults ?? throw new ArgumentNullException(nameof(countTestResults));
            this.saveTestResult = saveTestResult ?? throw new ArgumentNullException(nameof(saveTestResult));
        }

#warning изменить ImmutableList<ValidationResult> на исключения
        public IReadOnlyList<ValidationResult> Write(ResultTestDto dto)
        {
            var result = new TestResult()
            {
                Date = DateTime.Now,
                User = dto.User,
                Test = dto.Test,
                Answers = dto.Answers,
                Id = countTestResults.Get() + 1,
                Comment = dto.CommentFromTest,
                AdditionalComment = dto.AdditionalCommentForTest,
                Apparat = dto.Device,
                Release = dto.Release,
                UserId = dto.User.UserId,
            };

            saveTestResult.Write(result);

            return null;
        }
    }
}
