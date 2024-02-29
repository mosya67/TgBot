using Database.Database;
using Database.Database.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseServices
{
    public class DbServices
    {
        private List<ValidationResult> _errors = new();

        public bool ResetFio(long TgId, string fio)
        {
            var user = context.Users.SingleOrDefault(e => e.TgId == TgId);
            if (user != null)
            {
                user.Fio = fio;
                context.Update(user);
                context.SaveChanges();
                return true;
            }
            return false;
        }

        public ImmutableList<ValidationResult> SaveResult
            (AnswerDto dto, ImmutableList<ValidationResult> errors = null)
        {
            if (errors != null)
                _errors = new(_errors.Union(errors));

            var test = context.Tests.SingleOrDefault(p => p.Id == dto.testId);
            var valContext = new ValidationContext(dto);
            var countAnswers = CountAnswers();
            Validator.TryValidateObject(dto, valContext, _errors, true);
            for (int i = 0; i < dto.Answers.Count(); i++)
            {
                valContext = new ValidationContext(dto.Answers[i]);
                Validator.TryValidateObject(dto.Answers[i], valContext, _errors, true);
                dto.Answers[i].Id = countAnswers + i + 1;
            }

            if (test == null)
                _errors.Add(new("Test not found"));

            if(_errors.Count() == 0)
            {
                var user = context.Users.SingleOrDefault(e => e.TgId == dto.TgId) ?? new User
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
                    Id = context.TestResults.AsNoTracking().Count() + 1,
                    Comment = dto.CommentFromTest,
                    AdditionalComment = dto.AdditionalCommentForTest,
                    Apparat = dto.Device,
                    Release = dto.Release,
                    UserId = user.UserId,
                };
                context.TestResults.Add(result);

                context.SaveChanges();
            }

            return _errors.ToImmutableList();
        }

        public List<TestResult> GetData
            (DateTime? fdate, DateTime? ldate)
        {
            List<TestResult> results;
            if (fdate == null && ldate != null)
            {
                results = context.TestResults
                    .Include(p => p.Answers)
                    .Include(e => e.User)
                    .Include(e => e.Test)
                    .ThenInclude(p => p.Questions)
                    .AsNoTracking()
                    .Where(p => p.Date.Date <= ldate.Value.Date)
                    .ToList();
            }
            else if (ldate == null && fdate != null)
            {
                results = context.TestResults
                    .Include(p => p.Answers)
                    .Include(e => e.User)
                    .Include(e => e.Test)
                    .ThenInclude(p => p.Questions)
                    .AsNoTracking()
                    .Where(p => p.Date.Date >= fdate.Value.Date)
                    .ToList();
            }
            else if (ldate == null && fdate == null)
            {
                results = context.TestResults
                    .Include(p => p.Answers)
                    .Include(e => e.User)
                    .Include(e => e.Test)
                    .ThenInclude(p => p.Questions)
                    .AsNoTracking()
                    .ToList();
            }
            else
            {
                results = context.TestResults
                    .Include(p => p.Answers)
                    .Include(e => e.User)
                    .Include(e => e.Test)
                    .ThenInclude(p => p.Questions)
                    .AsNoTracking()
                    .Where(e => e.Date.Date >= fdate.Value.Date && e.Date.Date <= ldate.Value.Date)
                    .ToList();
            }

            return results;
        }
    }
}