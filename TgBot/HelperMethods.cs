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
using System.Runtime.CompilerServices;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    internal partial class Program
    {
        public static IStatusGeneric<DateTime> ParseDate(string message)
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

        public static void ComponentInitialization()
        {
            State = new Dictionary<long, UserState>();
        }

        public static void ScopedComponentInitialization()
        {
            Context context = new Context();

            excel = new ExcelGenerator(
                new GetDataForGeneratingReport(
                    context));

            getTest = new GetTest(
                context);

            getUserName = new GetUserName(
                context);

            getCountUsers = new GetCountAnswers(
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

        static async void DeleteButtonsAsync(ITelegramBotClient client, long id)
        {
            foreach (var button in State[id].deleteButtons)
            {
                await client.EditMessageReplyMarkupAsync(id, button);
            }
        }
    }
}
