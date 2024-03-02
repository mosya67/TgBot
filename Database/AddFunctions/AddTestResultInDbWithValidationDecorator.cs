using Database.Database.Model;
using Domain;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.AddFunctions
{
    public class AddTestResultInDbWithValidationDecorator : IWriteCommand<ImmutableList<ValidationResult>, ResultTestDTO>
    {
        readonly IGetCommand<int> countAnswers;
        readonly IWriteCommand<ImmutableList<ValidationResult>, ResultTestDTO> saveTestResult;

        public AddTestResultInDbWithValidationDecorator(IGetCommand<int> countAnswers, IWriteCommand<ImmutableList<ValidationResult>, ResultTestDTO> saveTestResult)
        {
            this.countAnswers = countAnswers ?? throw new ArgumentNullException(nameof(countAnswers));
            this.saveTestResult = saveTestResult ?? throw new ArgumentNullException(nameof(saveTestResult));
        }

        public ImmutableList<ValidationResult> Write(ResultTestDTO dto)
        {
            List<ValidationResult> _errors = new();

            if (dto.Errors != null)
                _errors = new(_errors.Union(dto.Errors));

            var _countAnswers = countAnswers.Get();
            var valContext = new ValidationContext(dto);

            Validator.TryValidateObject(dto, valContext, _errors, true);
            for (int i = 0; i < dto.Answers.Count(); i++)
            {
                valContext = new ValidationContext(dto.Answers[i]);
                Validator.TryValidateObject(dto.Answers[i], valContext, _errors, true);
                dto.Answers[i].Id = _countAnswers + i + 1;
            }


            if (_errors.Count() == 0)
            {
                saveTestResult.Write(dto);
            }

            return _errors.ToImmutableList();
        }
    }
}
