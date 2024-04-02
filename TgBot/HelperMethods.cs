using Database.AddFunctions;
using Database.Db;
using Database.GetFunctions;
using Domain.Model;
using ExcelServices;
using StatusGeneric;
using System;
using System.Collections.Generic;
using static TelegramBot.Keyboards;
using System.Linq;
using Telegram.Bot;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot;
using Domain.Dto;
using Domain;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

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

            getTest = new GetTest(context);

            getSortedTests = new GetSortedTests(context);

            var getCountAnswers = new GetCountAnswers(context);

            saveResult = new AddTestResultInDb(
                    new GetCountTestResults(context),
                    new UpdateResult(context),
                    getTest,
                    new GetUser(context),
                    new GetTestResult(context),
                    context);

            saveResultWithValidation = new AddTestResultInDbWithValidationDecorator(
                    getCountAnswers,
                    saveResult);

            getUsersPage = new GetUsersPage(context);

            addNewUser = new AddNewUser(context);

            getDevicesPage = new GetDevicesPage(context);

            addNewDevice = new AddNewDevice(context);

            getStoppedTest = new GetStoppedTests(context);

            getTestResult = new GetTestResult(context);

            updateTestResult = new UpdateResult(context);

            getTestJson = new GetTestJson(getTest);

            changeTest = new ChangeTest(new UpdateTest(context));

            getTestPage = new GetTestPage(context);
        }

        static async void SaveTestResult(ITelegramBotClient client, long id)
        {
            await client.SendTextMessageAsync(id, "тест завершен");
            if (State[id].PassingStoppedTest)
            {

                await updateTestResult.Write(new UpdateResultDto { answers = State[id].result.Answers, Id = State[id].ResultId, isStopped = false });
                await client.SendTextMessageAsync(id, "результат сохранен");
                State.Remove(id);
                return;
            }
            var errors = await saveResultWithValidation.Write(State[id].result);

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

        static async void NextQuestion(ITelegramBotClient client, long id, sbyte QuestNumb)
        {
            await client.SendTextMessageAsync(id,
                        State[id].Questions[QuestNumb].question +
                            (State[id].Questions[QuestNumb].Comment != null ? $"\nКомментарий: {State[id].Questions[QuestNumb].Comment}" : ""),
                        replyMarkup: answer);
        }

        static sbyte? GetNumberSkippedQuestion(long id, sbyte questNumb)
        {
            for (sbyte i = 0; i < State[id].result.Answers.Count(); i++)
            {
                if (State[id].result.Answers[i].Result == "LATER" && i >= questNumb)
                {
                    return i;
                }
            }
            return null;
        }

        static async void SendSkippedQuestion(ITelegramBotClient client, long id, sbyte? question)
        {
            await client.SendTextMessageAsync(id, "Ваш пропуск:");
            State[id].QuestNumb = question.Value;
            State[id].SkippedTestsFlag = true;
            State[id].ChatState = ChatState.AnswersTheQuestion;
            NextQuestion(client, id, State[id].QuestNumb);
        }

        static async void ViewTests(ITelegramBotClient client, long id, int mesId, IEnumerable<Test> tests)
        {
            await client.EditMessageReplyMarkupAsync(id, mesId);
            var testNamesAndDate = string.Join("\n", tests.Select(p => $"{p.Name}   {p.Date.ToShortDateString()}"));
            await client.SendTextMessageAsync(id, testNamesAndDate);
            var buttons = GetButtonsFromPageToSelectTest(0, tests.ToList(), e => e.Name, e => e.Id.ToString());
            await client.SendTextMessageAsync(id, "выберете тест", replyMarkup: buttons);
            State[id].ChatState = ChatState.SelectingTest;
        }

        static async void CheckTest(ITelegramBotClient client, long id, ushort testid, sbyte questNumb, int mesId)
        {
            await client.EditMessageReplyMarkupAsync(id, mesId);
            var test = await getTest.Get(testid);
            if (test == null)
            {
                await client.SendTextMessageAsync(id, "вы выбрали не существующий тест");
                return;
            }

            if (test.Questions.Count() == 0 || test.Questions == null)
            {
                DeleteButtons(client, id);
                await client.SendTextMessageAsync(id, "в тесте нет вопросов.\nвы можете выбрать другой тест");
                return;
            }

            State[id].Questions = test.Questions;
            State[id].result.Test = test;
            State[id].QuestNumb = questNumb;
        }

        static async Task<bool> HasStoppedTests(ITelegramBotClient client, long id, int mesId)
        {
            var stoppedTests = await getStoppedTest.Get(State[id].result.UserId);

            if (stoppedTests == null || stoppedTests.Count() == 0) return false;
            await client.EditMessageReplyMarkupAsync(id, mesId);
            var buttons = GetButtonsFromPageToContinuetesting(0, stoppedTests.ToList(), e => e.Id.ToString(), e => e.Id.ToString());

            var testNamesAndDate = string.Join("\n", stoppedTests.Select(p => $"{p.Id}   {p.Date.ToShortDateString()} {p.Date.ToShortTimeString()}"));
            await client.SendTextMessageAsync(id, "остановленные тестирования:" + '\n' + testNamesAndDate, replyMarkup: buttons);
            State[id].ChatState = ChatState.SelectingStoppedTest;
            return true;
        }

        static bool CheckSkippedQuestion(ITelegramBotClient client, long id, sbyte questNumb)
        {
            var question = GetNumberSkippedQuestion(id, questNumb);
            if (question.HasValue)
            {
                SendSkippedQuestion(client, id, question);
            }
            return question.HasValue;
        }

        static async void ChangePage(ITelegramBotClient client, long id, int mesId, Func<InlineKeyboardMarkup> getButtons)
        {
            var buttons = getButtons.Invoke();

            await client.EditMessageReplyMarkupAsync(id, mesId, replyMarkup: buttons);
        }

        static async void DeleteButtons(ITelegramBotClient client, long id)
        {
            if (State[id].deleteButtons == null) return;

            foreach (var mesId in State[id].deleteButtons)
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
            }

            State[id].deleteButtons = null;
        }

        static InlineKeyboardMarkup GetButtonsFromPage<T>(sbyte numberPage, IList<T> items, Func<T, string> ButtonName, Func<T, string> ButtonIdentificator)
        {
            var buttons = new List<List<InlineKeyboardButton>>();
            sbyte iterator = 0;
            for (int i = 0; i < BotSettings.heightButtonsOnMessage; i++)
            {
                buttons.Add(new List<InlineKeyboardButton>());
                for (int j = 0; j < BotSettings.widthButtonsOnMessage; j++, iterator++)
                {
                    if (iterator < items.Count())
                        buttons[i].Add(InlineKeyboardButton.WithCallbackData(ButtonName(items[iterator]), ButtonIdentificator(items[iterator])));
                }
            }

            AddButton<T>(ref buttons, "Добавить");

            var backAndNextButtons = new List<InlineKeyboardButton>();

            AddLastButton<T>(numberPage, ref backAndNextButtons);

            AddNextButton<T>(items, ref backAndNextButtons);
            buttons.Add(backAndNextButtons);

            return new InlineKeyboardMarkup(buttons);
        }
        static InlineKeyboardMarkup GetButtonsFromPageToContinuetesting<T>(sbyte numberPage, IList<T> items, Func<T, string> ButtonName, Func<T, string> ButtonIdentificator)
        {
            var buttons = new List<List<InlineKeyboardButton>>();
            sbyte iterator = 0;
            for (int i = 0; i < BotSettings.heightButtonsOnMessage; i++)
            {
                buttons.Add(new List<InlineKeyboardButton>());
                for (int j = 0; j < BotSettings.widthButtonsOnMessage; j++, iterator++)
                {
                    if (iterator < items.Count())
                        buttons[i].Add(InlineKeyboardButton.WithCallbackData(ButtonName(items[iterator]), ButtonIdentificator(items[iterator])));
                }
            }

            AddButton<T>(ref buttons, "начать тестирование");

            var backAndNextButtons = new List<InlineKeyboardButton>();

            AddLastButton<T>(numberPage, ref backAndNextButtons);

            AddNextButton<T>(items, ref backAndNextButtons);
            buttons.Add(backAndNextButtons);

            return new InlineKeyboardMarkup(buttons);
        }
        static InlineKeyboardMarkup GetButtonsFromPageToSelectTest<T>(sbyte numberPage, IList<T> items, Func<T, string> ButtonName, Func<T, string> ButtonIdentificator)
        {
            var buttons = new List<List<InlineKeyboardButton>>();
            sbyte iterator = 0;
            for (int i = 0; i < BotSettings.heightButtonsOnMessage; i++)
            {
                buttons.Add(new List<InlineKeyboardButton>());
                for (int j = 0; j < BotSettings.widthButtonsOnMessage; j++, iterator++)
                {
                    if (iterator < items.Count())
                        buttons[i].Add(InlineKeyboardButton.WithCallbackData(ButtonName(items[iterator]), ButtonIdentificator(items[iterator])));
                }
            }

            var backAndNextButtons = new List<InlineKeyboardButton>();

            AddLastButton<T>(numberPage, ref backAndNextButtons);

            AddNextButton<T>(items, ref backAndNextButtons);
            buttons.Add(backAndNextButtons);

            return new InlineKeyboardMarkup(buttons);
        }

        static async void StopTesting(long id, sbyte QuestNumb)
        {
            for (int i = QuestNumb; i < State[id].result.Answers.Count() && State[id].result.Answers[i].Result == null; i++)
            {
                State[id].result.Answers[i].Result = "LATER";
            }

            State[id].result.PausedQuestionNumber = State[id].QuestNumb;
            State[id].result.IsPaused = true;

            await saveResult.Write(State[id].result);
        }

        static void AddLastButton<T>(sbyte numberPage, ref List<InlineKeyboardButton> arr)
        {
            if (numberPage != 0)
            {
                arr.Add(InlineKeyboardButton.WithCallbackData("Назад", "Last" + typeof(T).Name + "Page"));
            }
        }

        static void AddNextButton<T>(IList<T> items, ref List<InlineKeyboardButton> arr)
        {
            if (items.Count() >= BotSettings.countElementsInPage)
            {
                arr.Add(InlineKeyboardButton.WithCallbackData("Далее", "Next" + typeof(T).Name + "Page"));
            }
        }

        static void AddButton<T>(ref List<List<InlineKeyboardButton>> arr, string buttonName)
        {
            var addUserButton = new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithCallbackData(buttonName, "AddNew" + typeof(T).Name),
            };

            arr.Add(addUserButton);
        }
    }
}
