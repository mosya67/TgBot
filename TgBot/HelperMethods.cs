﻿using Database.AddFunctions;
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
using OfficeOpenXml.FormulaParsing.Excel.Functions.Numeric;
using System.IO;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json;

namespace TelegramBot
{
    internal partial class Program
    {
        static IWriteCommand<Task<IReadOnlyList<ValidationResult>>, ResultTestDto> saveResultWithValidation;
        static IWriteCommand<Task<IReadOnlyList<ValidationResult>>, ResultTestDto> saveResult;
        static IGetCommand<Task<IEnumerable<TestResult>>, GetStoppedTestsDto> getStoppedTest;
        static IGetCommand<Task<IEnumerable<TestResult>>, StoppedTestResultPageDto> getStoppedTestsPage;
        static IGetCommand<Task<IEnumerable<Device>>, PageDto> getDevicesPage;
        static IGetCommand<Task<IEnumerable<User>>, PageDto> getUsersPage;
        static IGetCommand<Task<IEnumerable<Test>>, TestPageDto> getTestPage;
        static IExcelGenerator<Task<FileDto>, ReportExcelDTO> excel;
        static IGetCommand<Task<IEnumerable<Project>>, PageDto> getProjectPage;
        static IWriteCommand<Task, UpdateResultDto> updateTestResult;
        static IGetCommand<Task<TestResult>, ushort> getLastresult;
        static IGetCommand<Task<TestResult>, int> getTestResult;
        static IWriteCommand<Task, string> addNewDevice;
        static IGetCommand<Task<Test>, ushort> getTest;
        static IWriteCommand<Task, Stream> changeTest;
        static IWriteCommand<Task, string> addNewUser;
        static IWriteCommand<Task, Test> AddTest;
        static IWriteCommand<Task, Project> AddProject;
        static IGetCommand<Task<IList<Answer>>, LastResultDto> getLastAnswers;

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
            else status.AddError("некорректная дата");
            return status;
        }

        static void ComponentInitialization()
        {
            State = new Dictionary<long, UserState>();
        }

        static void ScopedComponentInitialization()
        {
            Context context = new Context();

            excel = new ExcelGenerator(new GetDataForGeneratingReport(context));

            getTest = new GetTest(context);

            AddTest = new AddTest(context);

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

            AddProject = new AddProject(context);

            getStoppedTestsPage = new GetStoppedTestsPage(context);

            getProjectPage = new GetProjectPage(context);

            addNewUser = new AddNewUser(context);

            getDevicesPage = new GetDevicesPage(context);

            addNewDevice = new AddNewDevice(context);

            getStoppedTest = new GetStoppedTests(context);

            getTestResult = new GetTestResult(context);

            updateTestResult = new UpdateResult(context);

            changeTest = new ChangeTest(new UpdateTest(new GetTest(context),
                context));

            getTestPage = new GetTestPage(context);

            getLastresult = new GetLastResult(context);

            getLastAnswers = new GetLastAnswersOnQuestion(context);
        }

        static async Task SaveTestResult(ITelegramBotClient client, long id)
        {
            await client.SendTextMessageAsync(id, "тест завершен");
            if (State[id].isPassingStoppedTest)
            {

                await updateTestResult.Write(new UpdateResultDto { answers = State[id].result.Answers, Id = State[id].ResultId, isStopped = false });
                await client.SendTextMessageAsync(id, "результат сохранен"); // сохранение результата после остановки и последующего продолжения тестирования
                State.Remove(id);
                return;
            }
            var errors = await saveResultWithValidation.Write(State[id].result); // обычное сохранение результата

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

        /// <summary>
        /// отправляет следующую проверку
        /// </summary>
        /// <returns></returns>
        static async Task SendQuestion(ITelegramBotClient client, long id, sbyte QuestNumb)
        {
            State[id].LastAnswers = await getLastAnswers.Get(
                new LastResultDto { QuestNumb = QuestNumb, resultsCount = set.CountLastResultsInCheck, testId = State[id].result.Test.Id });

            var lastResults_string = "";

            if (State[id].LastAnswers != null)
            {
                for (int i = 0; i < State[id].LastAnswers.Count(); i++)
                {
                    lastResults_string += i + 1 + ") Результат: " + State[id].LastAnswers[i].Result + (!string.IsNullOrEmpty(State[id].LastAnswers[i].Comment) ? '\n' + "Комментарий: " + State[id].LastAnswers[i].Comment : null) + '\n';
                }
            }

            await client.SendTextMessageAsync(id,
                        QuestNumb + 1 + ") " +
                        State[id].Questions[QuestNumb].question +
                        "\nОР: " + State[id].Questions[QuestNumb].ExpectedResult + 
                        (State[id].Questions[QuestNumb].Comment != null ? $"\nКомментарий: {State[id].Questions[QuestNumb].Comment}" : "") +
                        "\n\nПрошлые результаты:\n" + lastResults_string,
                        replyMarkup: answer);
        }

        /// <summary>
        /// отправляет следующую проверку, но с проверками на конец и тд
        /// </summary>
        /// <returns></returns>
        static async Task EndTestingOrNextQuestion(ITelegramBotClient client, long id)
        {
            if (!State[id].SkippedTestsFlag)
            {
                State[id].QuestNumb++;
                if (State[id].QuestNumb < State[id].Questions.Count())
                {
                    State[id].ChatState = ChatState.AnswersTheQuestion; // <---- просто продолжение прохождение вопросов (не пропущенных)
                    await SendQuestion(client, id, State[id].QuestNumb);
                    return;
                }
                State[id].QuestNumb = 0;

                bool check1 = await CheckSkippedQuestion(client, id, State[id].QuestNumb); // <---- возможно костыль \\ нужен для того, что бы вывод первого скипа работал нормально
                if (check1) return; // тк без него вывод начинается со 2-го, а без инкремента, который чуть ниже, он зацикливается на 1-м
                // при возобновлении тестирования сюда не попадает
            }

            bool check2 = await CheckSkippedQuestion(client, id, ++State[id].QuestNumb);
            if (check2) return;

            bool check3 = await CheckSkippedQuestion(client, id, 0);// <---- проверка на то, что после прохождения скипов, у человек не нажимал снова скипы (работает после прохождения последней проверки)
            if (check3) return;

            State[id].datesDto.variant = 1;
            State[id].datesDto.TestId = State[id].result.Test.Id;
            var temp = State[id].datesDto;

            await SaveTestResult(client, id);

            var file = await excel.WriteResultsAsync(temp);
            await SendAndDeleteDocument(client, id, file.PathName);
        }

        /// <summary>
        /// получает номер проверки, где ответом был "LATER"
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// отправка пропущенной проверки
        /// </summary>
        /// <returns></returns>
        static async Task SendSkippedQuestion(ITelegramBotClient client, long id, sbyte? question)
        {
            await client.SendTextMessageAsync(id, "Ваш пропуск:");
            State[id].QuestNumb = question.Value;
            State[id].SkippedTestsFlag = true;
            State[id].ChatState = ChatState.AnswersTheQuestion;
            await SendQuestion(client, id, State[id].QuestNumb);
        }

        /// <summary>
        /// отправляет документ пользователю, полсе чего удаляет его
        /// </summary>
        /// <param name="path">путь к файлу</param>
        /// <returns></returns>
        static async Task SendAndDeleteDocument(ITelegramBotClient client, long id, string path)
        {
            await Task.Run(async () =>
            {
                using (Stream str = File.OpenRead(path))
                {
                    await client.SendDocumentAsync(id, new(str, Path.GetFileName(path)));
                }
            }).ContinueWith(e =>
            {
                File.Delete(path);
            });
        }

        /// <summary>
        /// отправляет документ пользователю
        /// </summary>
        /// <param name="path">путь к файлу</param>
        /// <returns></returns>
        static async Task SendDocument(ITelegramBotClient client, long id, string path)
        {
            await Task.Run(async () =>
            {
                using (Stream str = File.OpenRead(path))
                {
                    await client.SendDocumentAsync(id, new(str, Path.GetFileName(path)));
                }
            });
        }

        static async Task resetsettings(string path)
        {
            var s = new BotSettings { CountLastResultsInCheck = 3, heightButtonsOnPage = 2, widthButtonsOnPage = 2 };

            set = s;

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
            string json = JsonConvert.SerializeObject(s, settings);
            await File.WriteAllTextAsync(path, json);
        }

        /// <summary>
        /// выводит пользователю список чек-листов, которые были переданы в tests
        /// </summary>
        /// <param name="tests">массив чек-листов</param>
        /// <returns></returns>
        static async Task ViewTests(ITelegramBotClient client, long id, IEnumerable<Test> tests)
        {
            var buttons = GetButtonsFromPageToSelectTest(State[id].Role, 0, tests.ToList(), e => $"{e.Name}", e => e.Id.ToString());
            if (tests.Count() == 0)
            {
                await client.SendTextMessageAsync(id, "Тестов нет", replyMarkup: buttons);
                return;
            }
            await client.SendTextMessageAsync(id, "выберете тест", replyMarkup: buttons);
            State[id].ChatState = ChatState.SelectingTest;
        }

        /// <summary>
        /// выводит пользователю список проектов, которые были переданы в testGrops
        /// </summary>
        /// <param name="testGrops">массив чек-листов</param>
        /// <returns></returns>
        async static Task ViewProjects(ITelegramBotClient client, long id, IEnumerable<Project> testGrops)
        {
            var buttons = GetButtonsFromPageToSelectTest(State[id].Role, 0, testGrops.ToList(), e => $"{e.Name}", e => e.Id.ToString());
            if (testGrops.Count() == 0)
            {
                await client.SendTextMessageAsync(id, "Проектов нет", replyMarkup: buttons);
                return;
            }
            await client.SendTextMessageAsync(id, "Выберете проект", replyMarkup: buttons);
            State[id].ChatState = ChatState.SelectingProject;
        }

        async static Task ViewProjectsForReport(ITelegramBotClient client, long id, IEnumerable<Project> testGrops)
        {
            var buttons = GetButtonsFromPageToSelectTest(UserRole.None, 0, testGrops.ToList(), e => $"{e.Name}", e => e.Id.ToString());
            if (testGrops.Count() == 0)
            {
                await client.SendTextMessageAsync(id, "Проектов нет", replyMarkup: buttons);
                return;
            }
            await client.SendTextMessageAsync(id, "Выберете проект", replyMarkup: buttons);
            State[id].ChatState = ChatState.SelectingProjectForReport;
        }

        /// <summary>
        /// выводит пользователю список чек-листов для получения ecxcel файла, которые были переданы в tests
        /// </summary>
        /// <param name="tests">массив чек-листов</param>
        /// <returns></returns>
        static async Task ViewTestsForReports(ITelegramBotClient client, long id, IEnumerable<Test> tests)
        {
            var buttons = GetButtonsFromPageToReports(0, tests.ToList(), e => $"{e.Name}", e => e.Id.ToString());
            if (tests.Count() == 0)
            {
                await client.SendTextMessageAsync(id, "Тестов нет", replyMarkup: buttons);
                return;
            }
            await client.SendTextMessageAsync(id, "выберете тест", replyMarkup: buttons);
            State[id].ChatState = ChatState.SelectingTestToReports;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static async Task CheckTest(ITelegramBotClient client, long id, ushort testid, sbyte questNumb, int mesId)
        {
            await client.EditMessageReplyMarkupAsync(id, mesId);
            var test = await getTest.Get(testid);
            State[id].result.TestVersionId = test.TestVersionId;

            if (test.Questions.Count() == 0 || test.Questions == null)
            {
                
                await client.SendTextMessageAsync(id, "В тесте нет проверок.\nВы можете выбрать другой тест");
                return;
            }

            State[id].Questions = test.Questions;
            State[id].result.Test = test;
            State[id].QuestNumb = questNumb;
        }

        /// <summary>
        /// проверка на наличие остановленных тестирований
        /// </summary>
        /// <returns></returns>
        static async Task<bool> HasStoppedTests(ITelegramBotClient client, long id, int mesId)
        {
            var stoppedTests = await getStoppedTest.Get(new GetStoppedTestsDto { userId = State[id].result.UserId, testId = State[id].result.Test.Id });

            if (stoppedTests == null || stoppedTests.Count() == 0) return false;
            await client.EditMessageReplyMarkupAsync(id, mesId);
            var buttons = GetButtonsFromPageToContinueTesting(UserRole.Admin, 0, stoppedTests.ToList(), e => e.Date.ToShortDateString() + ' ' + e.Date.ToShortTimeString(), e => e.Id.ToString());
            await client.SendTextMessageAsync(id, "Остановленные тестирования (названия кнопок - дата остановки):", replyMarkup: buttons);
            State[id].ChatState = ChatState.SelectingStoppedTest;
            return true;
        }

        /// <summary>
        /// проверка на наличие пропущенных проверок
        /// </summary>
        /// <returns></returns>
        static async Task<bool> CheckSkippedQuestion(ITelegramBotClient client, long id, sbyte questNumb)
        {
            var question = GetNumberSkippedQuestion(id, questNumb);
            if (question.HasValue)
            {
                await SendSkippedQuestion(client, id, question);
            }
            return question.HasValue;
        }

        /// <summary>
        /// переключает страницу
        /// </summary>
        /// <param name="getButtons">функция, которая возвращает кнопки для для бота</param>
        /// <returns></returns>
        static async Task ChangePage(ITelegramBotClient client, long id, int mesId, Func<InlineKeyboardMarkup> getButtons)
        {
            var buttons = getButtons.Invoke();

            await client.EditMessageReplyMarkupAsync(id, mesId, replyMarkup: buttons);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static async Task DeleteButtons(ITelegramBotClient client, long id)
        {
            if (State[id].deleteButtons == null || State[id].deleteButtons.Count() == 0) return;

            foreach (var mesId in State[id].deleteButtons)
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
            }

            State[id].deleteButtons = null;
        }

        static InlineKeyboardMarkup GetButtonsFromPage<T>(UserRole role, sbyte numberPage, IList<T> items, Func<T, string> ButtonName, Func<T, string> ButtonIdentificator)
        {
            var buttons = new List<List<InlineKeyboardButton>>();
            sbyte iterator = 0;
            for (int i = 0; i < set.heightButtonsOnPage; i++)
            {
                buttons.Add(new List<InlineKeyboardButton>());
                for (int j = 0; j < set.widthButtonsOnPage; j++, iterator++)
                {
                    if (iterator < items.Count())
                        buttons[i].Add(InlineKeyboardButton.WithCallbackData(ButtonName(items[iterator]), ButtonIdentificator(items[iterator])));
                }
            }


            var backAndNextButtons = new List<InlineKeyboardButton>();

            AddLastButton<T>(numberPage, ref backAndNextButtons);

            AddNextButton<T>(items, ref backAndNextButtons);
            buttons.Add(backAndNextButtons);
            AddButton<T>(role, ref buttons, "Добавить");

            return new InlineKeyboardMarkup(buttons);
        }
        static InlineKeyboardMarkup GetButtonsFromAnwer(long id)
        {
            var buttons = new List<InlineKeyboardButton>();
            if (State[id].LastAnswers != null)
            {
                for (sbyte i = 0; i < State[id].LastAnswers.Count(); i++)
                {
                    buttons.Add(InlineKeyboardButton.WithCallbackData((i + 1).ToString(), i.ToString()));
                }
            }

            buttons.Add(InlineKeyboardButton.WithCallbackData("Назад", "WriteLastResult_BackButton"));

            return new InlineKeyboardMarkup(buttons);
        }
        static InlineKeyboardMarkup GetButtonsFromPageToContinueTesting<T>(UserRole role, sbyte numberPage, IList<T> items, Func<T, string> ButtonName, Func<T, string> ButtonIdentificator)
        {
            var buttons = new List<List<InlineKeyboardButton>>();
            sbyte iterator = 0;
            for (int i = 0; i < set.heightButtonsOnPage; i++)
            {
                buttons.Add(new List<InlineKeyboardButton>());
                for (int j = 0; j < set.widthButtonsOnPage; j++, iterator++)
                {
                    if (iterator < items.Count())
                        buttons[i].Add(InlineKeyboardButton.WithCallbackData(ButtonName(items[iterator]), ButtonIdentificator(items[iterator])));
                }
            }


            var backAndNextButtons = new List<InlineKeyboardButton>();

            AddLastButton<T>(numberPage, ref backAndNextButtons);

            AddNextButton<T>(items, ref backAndNextButtons);
            buttons.Add(backAndNextButtons);
            AddButton<T>(role, ref buttons, "Начать тестирование");

            return new InlineKeyboardMarkup(buttons);
        }
        static InlineKeyboardMarkup GetButtonsFromPageToSelectTest<T>(UserRole role, sbyte numberPage, IList<T> items, Func<T, string> ButtonName, Func<T, string> ButtonIdentificator)
        {
            var buttons = new List<List<InlineKeyboardButton>>();
            sbyte iterator = 0;
            for (int i = 0; i < set.heightButtonsOnPage; i++)
            {
                buttons.Add(new List<InlineKeyboardButton>());
                for (int j = 0; j < set.widthButtonsOnPage; j++, iterator++)
                {
                    if (iterator < items.Count())
                        buttons[i].Add(InlineKeyboardButton.WithCallbackData(ButtonName(items[iterator]), ButtonIdentificator(items[iterator])));
                }
            }

            var backAndNextButtons = new List<InlineKeyboardButton>();


            AddLastButton<T>(numberPage, ref backAndNextButtons);

            AddNextButton<T>(items, ref backAndNextButtons);
            buttons.Add(backAndNextButtons);
            AddButton<T>(role, ref buttons, "Добавить");

            return new InlineKeyboardMarkup(buttons);
        }

        static InlineKeyboardMarkup GetButtonsFromPageToReports<T>(sbyte numberPage, IList<T> items, Func<T, string> ButtonName, Func<T, string> ButtonIdentificator)
        {
            var buttons = new List<List<InlineKeyboardButton>>();
            sbyte iterator = 0;
            for (int i = 0; i < set.heightButtonsOnPage; i++)
            {
                buttons.Add(new List<InlineKeyboardButton>());
                for (int j = 0; j < set.widthButtonsOnPage; j++, iterator++)
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

        /// <summary>
        /// метод останавливает тестирование
        /// </summary>
        /// <returns></returns>
        static async Task StopTesting(long id, sbyte QuestNumb)
        {
            for (int i = QuestNumb; i < State[id].result.Answers.Count() && State[id].result.Answers[i].Result == null; i++)
            {
                State[id].result.Answers[i].Result = "LATER";
            }

            State[id].result.PausedQuestionNumber = State[id].QuestNumb;
            State[id].result.IsPaused = true;

            await saveResult.Write(State[id].result);
        }

        /// <summary>
        /// добавляет кнопку "назад"
        /// </summary>
        /// <param name="arr">массив кнопок куда будет добавлена кнопка "назад"</param>
        static void AddLastButton<T>(sbyte numberPage, ref List<InlineKeyboardButton> arr)
        {
            if (numberPage != 0)
            {
                arr.Add(InlineKeyboardButton.WithCallbackData("Назад", "Last" + typeof(T).Name + "Page"));
            }
        }

        /// <summary>
        /// добавляет кнопку "далее"
        /// </summary>
        /// <param name="arr">массив кнопок куда будет добавлена кнопка "далее"</param>
        /// <param name="items">элементы в страницу</param>
        static void AddNextButton<T>(IList<T> items, ref List<InlineKeyboardButton> arr)
        {
            if (items.Count() > set.countElementsInPage)
            {
                arr.Add(InlineKeyboardButton.WithCallbackData("Далее", "Next" + typeof(T).Name + "Page"));
            }
        }

        /// <summary>
        /// добавляет кнопку "добавить". (необязательно именно ее, тк имя задается из вне, но обрабатывать ее все равно придется через "AddNew" + typeof(T).Name)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="role">роль, выбранная пользователем</param>
        /// <param name="arr">массив в который мы добавим кнопку</param>
        /// <param name="buttonName">имя кнопки</param>
        static void AddButton<T>(UserRole role, ref List<List<InlineKeyboardButton>> arr, string buttonName)
        {
            if (role == UserRole.Admin)
            {
                var addUserButton = new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData(buttonName, "AddNew" + typeof(T).Name),
                };

                arr.Add(addUserButton);
            }
        }
    }
}
