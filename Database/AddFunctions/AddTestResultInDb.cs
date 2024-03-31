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
        readonly IGetCommand<Test, ushort> getTest;
        readonly IGetCommand<User, long> getUser;

        public AddTestResultInDb(IGetCommand<int> countTestResults, IWriteCommand<TestResult> saveTestResult, IGetCommand<Test, ushort> getTest, IGetCommand<User, long> getUser)
        {
            this.countTestResults = countTestResults ?? throw new ArgumentNullException(nameof(countTestResults));
            this.saveTestResult = saveTestResult ?? throw new ArgumentNullException(nameof(saveTestResult));
            this.getTest = getTest ?? throw new ArgumentNullException(nameof(getTest));
            this.getUser = getUser ?? throw new ArgumentNullException(nameof(getUser));
        }


        public IReadOnlyList<ValidationResult> Write(ResultTestDto dto)
        {
            var user = getUser.Get(dto.UserId) ?? new User
            {
                Fio = dto.UserName,
                TgId = dto.UserId,
            };

            var result = new TestResult()
            {
                Date = DateTime.Now,
                User = user,
                Test = getTest.Get(dto.Test.Id),
                Answers = dto.Answers,
                Id = countTestResults.Get() + 1,
                Comment = dto.CommentFromTest,
                AdditionalComment = dto.AdditionalCommentForTest,
                Apparat = dto.Device,
                Version = dto.Version,
            };

            saveTestResult.Write(result);

            return null;
        }
    }
}
