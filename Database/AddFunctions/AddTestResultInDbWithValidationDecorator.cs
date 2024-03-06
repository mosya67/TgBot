using Domain;
using Domain.Dto;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.AddFunctions
{
    public class AddTestResultInDbWithValidationDecorator : IWriteCommand<IReadOnlyList<ValidationResult>, ResultTestDto>
    {
        readonly IGetCommand<int> countAnswers;
        readonly IWriteCommand<IReadOnlyList<ValidationResult>, ResultTestDto> saveTestResult;

        public AddTestResultInDbWithValidationDecorator(IGetCommand<int> countAnswers, IWriteCommand<IReadOnlyList<ValidationResult>, ResultTestDto> saveTestResult)
        {
            this.countAnswers = countAnswers ?? throw new ArgumentNullException(nameof(countAnswers));
            this.saveTestResult = saveTestResult ?? throw new ArgumentNullException(nameof(saveTestResult));
        }

        public IReadOnlyList<ValidationResult> Write(ResultTestDto dto)
        {
            List<ValidationResult> _errors = new ();

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

            return _errors;
        }
    }
}
