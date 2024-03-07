using Database.AddFunctions;
using Database.Db;
using Database.GetFunctions;
using Domain.Model;
using ExcelServices;
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

            getUser = new GetUser(
                context);

            getCountUsers = new GetCountAnswers(
                context);

            getSortedTests = new GetSortedTests(
                context);

            saveResult = new AddTestResultInDbWithValidationDecorator(
                new GetCountAnswers(
                    context),
                new AddTestResultInDbWithValidationDecorator(
                    new GetCountAnswers(
                        context),
                    new AddTestResultInDb(
                        new GetCountTestResults(
                            context),
                    new AddObjectInDb<TestResult>(
                        context))));
        }
    }
}
