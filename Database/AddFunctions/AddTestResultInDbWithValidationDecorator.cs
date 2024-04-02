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
    public class AddTestResultInDbWithValidationDecorator : IWriteCommand<Task<IReadOnlyList<ValidationResult>>, ResultTestDto>
    {
        readonly IGetCommand<Task<int>> countAnswers;
        readonly IWriteCommand<Task<IReadOnlyList<ValidationResult>>, ResultTestDto> saveTestResult;

        public AddTestResultInDbWithValidationDecorator(IGetCommand<Task<int>> countAnswers, IWriteCommand<Task<IReadOnlyList<ValidationResult>>, ResultTestDto> saveTestResult)
        {
            this.countAnswers = countAnswers ?? throw new ArgumentNullException(nameof(countAnswers));
            this.saveTestResult = saveTestResult ?? throw new ArgumentNullException(nameof(saveTestResult));
        }

        public async Task<IReadOnlyList<ValidationResult>> Write(ResultTestDto dto)
        {
            List<ValidationResult> _errors = new List<ValidationResult>();

            var _countAnswers = await countAnswers.Get();
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
                await saveTestResult.Write(dto);
            }

            return _errors;
        }
    }
}
