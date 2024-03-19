using Database.AddFunctions;
using Database.Db;
using Database.GetFunctions;
using Domain.Model;
using ExcelServices;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Numeric;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using StatusGeneric;
using System;
using System.Collections.Generic;
using static TelegramBot.Keyboards;
using System.Linq;
using System.Runtime.CompilerServices;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;

namespace TelegramBot
{
    internal partial class Program
    {
        static IStatusGeneric<DateTime> ParseDate(string message)
        {
            DateTime date;
            var status = new StatusGenericHandler<DateTime>();
            if (DateTime.TryParse(message, out date))
            {
                if (date == default)
                {
                    status.AddError("некорректная дата");
                    return status;
                }

                status.SetResult(date);
            }
            return status;
        }

        static void ComponentInitialization()
        {
            State = new Dictionary<long, UserState>();
        }

        static void ScopedComponentInitialization()
        {
            Context context = new Context();

            excel = new ExcelGenerator(
                new GetDataForGeneratingReport(
                    context));

            getTest = new GetTest(
                context);

            getUserName = new GetUserName(
                context);

            getSortedTests = new GetSortedTests(
                context);

            var getCountAnswers = new GetCountAnswers(
                    context);

            saveResult = new AddTestResultInDbWithValidationDecorator(
                getCountAnswers,
                new AddTestResultInDbWithValidationDecorator(
                    getCountAnswers,
                    new AddTestResultInDb(
                        new GetCountTestResults(
                            context),
                    new AddObjectInDb<TestResult>(
                        context),
                    getTest,
                    new GetUser(
                        context))));
        }

        static async void SaveTestResult(ITelegramBotClient client, long id)
        {
            await client.SendTextMessageAsync(id, "тест завершен"); // <---- попал как только закончились скипнутые вопросы у пользователя
            var errors = saveResult.Write(State[id].result);

            if (errors.Count() != 0)
            {
                await client.SendTextMessageAsync(id, "не удалось сохранить результат");
                string Errors = string.Join("\n", errors);
                await client.SendTextMessageAsync(id, $"Ошибки:\n{Errors}");
            }
            else
                await client.SendTextMessageAsync(id, "результат сохранен");

            State.Remove(id);
        }

        static async void NextQuestion(ITelegramBotClient client, long id, int QuestNumb)
        {
            await client.SendTextMessageAsync(id,
                        State[id].Questions[QuestNumb].Question1 +
                            (State[id].Questions[QuestNumb].Comment != null ? $"\nКомментарий: {State[id].Questions[QuestNumb].Comment}" : ""),
                        replyMarkup: answer);
        }

        static ushort? GetNumberSkippedQuestion(ITelegramBotClient client, long id)
        {
            for (ushort i = 0; i < State[id].result.Answers.Count(); i++)
            {
                if (State[id].result.Answers[i].Result == "LATER")
                {
                    return i;
                }
            }

            return null;
        }

        static ushort? GetNumberSkippedQuestion(ITelegramBotClient client, long id, ushort questNumb)
        {
            for (ushort i = 0; i < State[id].result.Answers.Count(); i++)
            {
                if (State[id].result.Answers[i].Result == "LATER" && i >= questNumb)
                {
                    return i;
                }
            }
            return null;
        }

        static async void SendSkippedQuestion(ITelegramBotClient client, long id, ushort? question)
        {
            await client.SendTextMessageAsync(id, "Ваш пропуск:");
            State[id].QuestNumb = question.Value;
            State[id].SkippedTestsFlag = true;
            State[id].ChatState = ChatState.AnswersTheQuestion;
            NextQuestion(client, id, State[id].QuestNumb);
        }

        static bool CheckSkippedQuestion(ITelegramBotClient client, long id, ushort questNumb)
        {
            var question = GetNumberSkippedQuestion(client, id, questNumb);
            if (question.HasValue)
            {
                SendSkippedQuestion(client, id, question);
            }
            return question.HasValue;
        }
    }
}
