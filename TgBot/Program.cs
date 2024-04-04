using Telegram.Bot;
using Telegram.Bot.Types;
using static TelegramBot.Keyboards;
using Telegram.Bot.Types.Enums;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.IO;
using Domain;
using Domain.Dto;
using Domain.Model;
using System.ComponentModel.DataAnnotations;
using TgBot;
using File = System.IO.File;
using Newtonsoft.Json;
using Database.Db;
using Microsoft.EntityFrameworkCore;

namespace TelegramBot
{
    internal partial class Program
    {
        static Dictionary<long, UserState> State;

        static IWriteCommand<Task<IReadOnlyList<ValidationResult>>, ResultTestDto> saveResultWithValidation;
        static IWriteCommand<Task<IReadOnlyList<ValidationResult>>, ResultTestDto> saveResult;
        static IGetCommand<Task<IEnumerable<Domain.Model.User>>, PageDto> getUsersPage;
        static IGetCommand<Task<IEnumerable<TestResult>>, long> getStoppedTest;
        static IGetCommand<Task<IEnumerable<Device>>, PageDto> getDevicesPage;
        static IGetCommand<Task<IEnumerable<Test>>, PageDto> getTestPage;
        static IExcelGenerator<Task<FileDto>, ReportExcelDTO> excel;
        static IWriteCommand<Task, UpdateResultDto> updateTestResult;
        static IGetCommand<Task<TestResult>, ushort> getLastresult;
        static IGetCommand<Task<TestResult>, int> getTestResult;
        static IGetCommand<Task<string>, ushort> getTestJson;
        static IWriteCommand<Task, string> addNewDevice;
        static IGetCommand<Task<Test>, ushort> getTest;
        static IWriteCommand<Task, Stream> changeTest;
        static IWriteCommand<Task, string> addNewUser;
        static IWriteCommand<Task, Test> AddTest;

        static async Task Main(string[] args)
        {
            ComponentInitialization();

            var proxy = new WebProxy
            {
                //Address = new Uri($"http://gw-srv.elektron.spb.su:3128"),
                //BypassProxyOnLocal = false,
                //UseDefaultCredentials = false,
                //Credentials = new NetworkCredential(
                //    userName: "Mostyaev_AS",
                //    password: "mostaevartem12345--")
            };
            var Httpclient = new HttpClient(handler: new HttpClientHandler { Proxy = proxy }, disposeHandler: true);
            var client = new TelegramBotClient(BotSettings.token, Httpclient);
            client.StartReceiving(Update, Error);
            Console.WriteLine("Bot started");
            Console.Read();
        }

        async public static Task Update(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Type == UpdateType.Message)
            {
                ScopedComponentInitialization();
                await HandleMessage(client, update.Message);
            }
            else if (update?.Type == UpdateType.CallbackQuery)
            {
                ScopedComponentInitialization();
                await HandleCallBackQuery(client, update.CallbackQuery);
            }
        }

        private static Task Error<T>(ITelegramBotClient client, T ex, CancellationToken token)
            where T : Exception
        {
            string message = '\n' + ex.GetType().Name + '\n' + ex.Message + '\n' + ex.InnerException?.Message + '\n' + ex.StackTrace;
            Console.WriteLine(message);
            throw new Exception(message);
        }

        async static Task HandleMessage(ITelegramBotClient client, Message message)
        {
            var id = message.Chat.Id;
            Console.WriteLine($"{message.Date} {message.From.Username} {message.From.Id}: {message.Text}");
            if (!State.ContainsKey(id))
                State.Add(id, new UserState());

            if (message?.Text?.ToLower() == "/start")
            {
                State[id].ChatState = ChatState.None;
                State[id].SkippedTestsFlag = false;
                await client.SendTextMessageAsync(id, "Список комманд: /commands");
                await client.SendTextMessageAsync(id, "вы можете выбрать тест или получить отчеты", replyMarkup: start);
                return;
            }
#if DEBUG
            else if (message?.Text?.ToLower() == "/s")
            {
                var c = new Context();
                var vers = c.TestVersions.Include(e => e.Questions).ToList();

                foreach (var i in vers)
                {
                    var test = await getTest.Get(i.TestId);
                    Console.WriteLine($"{test.Name} {i.DateCreated}");
                    foreach (var q in i.Questions)
                    {
                        Console.WriteLine($"    {q.question}");
                    }
                }
            }
#endif
            else if (State[id].ChatState == ChatState.SetFio)
            {
                State[id].result.UserName = message.Text;
                await client.SendTextMessageAsync(id, "введите комментарий к тесту[необязательно]", replyMarkup: NextCom);
                State[id].ChatState = ChatState.CommentForTest;
            }
            else if (State[id].ChatState == ChatState.AddNewUser)
            {
                DeleteButtons(client, id);
                await addNewUser.Write(message.Text);
                var users = await getUsersPage.Get(new PageDto() { startPage = State[id].NumerPage, countElementsInPage = BotSettings.countElementsInPage });
                var buttons = GetButtonsFromPage(State[id].NumerPage, users.ToList(), e => e.Fio, e => e.TgId.ToString());
                Message msg = await client.SendTextMessageAsync(id, "Выберите пользователя:", replyMarkup: buttons);
                State[id].deleteButtons = new List<int>() { msg.MessageId };
                State[id].ChatState = ChatState.SelectionUser;
            }
            else if (State[id].ChatState == ChatState.AddNewDevice)
            {
                DeleteButtons(client, id);
                await addNewDevice.Write(message.Text);
                var devicesPage = await getDevicesPage.Get(new PageDto() { startPage = State[id].NumerPage, countElementsInPage = BotSettings.countElementsInPage });
                var buttons = GetButtonsFromPage(State[id].NumerPage, devicesPage.ToList(), e => e.Name, e => e.Name);
                Message msg = await client.SendTextMessageAsync(id, "Выберите аппарат:", replyMarkup: buttons);
                State[id].deleteButtons = new List<int>() { msg.MessageId };
                State[id].ChatState = ChatState.SelectionDevice;
            }
            else if (State[id].ChatState == ChatState.Commenting)
            {
                if (!State[id].SkippedTestsFlag)
                {
                    State[id].result.Answers[State[id].QuestNumb].Comment = message.Text;
                    if (State[id].QuestNumb == State[id].Questions.Count() - 1)
                    {
                        State[id].SkippedTestsFlag = false;
                    }
                }
                else
                    State[id].result.Answers[State[id].QuestNumb].Comment = message.Text;
                State[id].ChatState = ChatState.None;
            }
            else if (State[id].ChatState == ChatState.FirtsDate)
            {
                var date = ParseDate(message.Text);
                if (date.HasErrors)
                {
                    await client.SendTextMessageAsync(id, $"Что-то пошло не так. Попробуйте снова\n{string.Join('\n', date.Errors.Select(p => p.ErrorResult.ErrorMessage))}");
                    return;
                }
                State[id].datesDto.fdate = date.Result;
                State[id].ChatState = ChatState.LastDate;
                await client.SendTextMessageAsync(id, "конечная дата:", replyMarkup: skipldate);
            }
            else if (State[id].ChatState == ChatState.LastDate)
            {
                var date = ParseDate(message.Text);
                if (date.HasErrors)
                {
                    await client.SendTextMessageAsync(id, $"Что-то пошло не так. Попробуйте снова\n{string.Join('\n', date.Errors.Select(p => p.ErrorResult.ErrorMessage))}");
                    return;
                }
                State[id].datesDto.ldate = date.Result;

                var file = await excel.WriteResultsAsync(State[id].datesDto);
                if (file.Errors == null)
                {
                    using (Stream str = File.OpenRead(file.PathName))
                    {
                        await client.SendDocumentAsync(id, new(str, Path.GetFileName(file.PathName)));
                    }
                    State[id].ChatState = ChatState.None;
                    return;
                }
                await client.SendTextMessageAsync(id, string.Join('\n', file.Errors));
            }
            else if (State[id].ChatState == ChatState.CommentForTest)
            {
                State[id].result.CommentFromTest = message.Text;
            }
            else if (State[id].ChatState == ChatState.AdditionalCommentForTest)
            {
                State[id].result.AdditionalCommentForTest = message.Text;
            }
            else if (State[id].ChatState == ChatState.Release)
            {
                State[id].result.Version = message.Text;
            }
            else if (State[id].ChatState == ChatState.AddTest && message.Type == MessageType.Document)
            {
                var file = await client.GetFileAsync(message.Document.FileId);

                using (var stream = new MemoryStream())
                {
                    await client.DownloadFileAsync(file.FilePath, stream);

                    try
                    {
                        stream.Position = 0;
                        using (var reader = new StreamReader(stream))
                        {
                            string jsonContent = await reader.ReadToEndAsync();

                            var test = JsonConvert.DeserializeObject<Test>(jsonContent);

                            await AddTest.Write(test);
                        }
                        await client.SendTextMessageAsync(id, $"готово");
                        State[id].result.Answers = new List<Answer>();
                        var tests = await getTestPage.Get(new PageDto { countElementsInPage = BotSettings.countElementsInPage, startPage = 0 });
                        ViewTests(client, id, message.MessageId, tests, false);
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(id, $"не удалось запарсить файл\n" + ex.Message + '\n' + ex.InnerException?.Message);
                    }
                }
            }
            else if (State[id].ChatState == ChatState.ChangeTest && message.Type == MessageType.Document)
            {
                var file = await client.GetFileAsync(message.Document.FileId);

                using (var stream = new MemoryStream())
                {
                    await client.DownloadFileAsync(file.FilePath, stream);

                    try
                    {
                        await changeTest.Write(stream);
                        await client.SendTextMessageAsync(id, $"готово");
                        State[id].result.Answers = new List<Answer>();
                        var tests = await getTestPage.Get(new PageDto { countElementsInPage = BotSettings.countElementsInPage, startPage = 0 });
                        ViewTests(client, id, message.MessageId, tests, false);
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(id, $"не удалось запарсить файл\n" + ex.Message + '\n' + ex.InnerException?.Message);
                    }
                }

            }
        }

        async static Task HandleCallBackQuery(ITelegramBotClient client, CallbackQuery query)
        {
            var id = query.Message.Chat.Id;
            ushort testid;
            if (!State.ContainsKey(id)) return;
            var mesId = query.Message.MessageId;

            Console.WriteLine($"{query.Message.Date} {query.From.Username} {query.From.Id}: {query.Data}");

            if (State[id].ChatState == ChatState.AnswersTheQuestion)
            {
                if (query.Data == "PAUSETEST")
                {
                    State[id].result.TestResultId = State[id].ResultId;
                    StopTesting(id, State[id].QuestNumb);
                    await client.SendTextMessageAsync(id, "тест был остановлен");
                    var tests = await getTestPage.Get(new PageDto { countElementsInPage = BotSettings.countElementsInPage, startPage = 0 });
                    ViewTests(client, id, mesId, tests);
                    return;
                }

                State[id].ChatState = ChatState.Commenting;
                State[id].result.Answers[State[id].QuestNumb].Result = query.Data;
                await client.EditMessageTextAsync(id, mesId, "Введите комментарий[необязательно]", replyMarkup: next);
            }
            else if (query.Data == "NextCom")
            {
                State[id].ChatState = ChatState.AdditionalCommentForTest;
                await client.EditMessageReplyMarkupAsync(id, mesId);
                await client.SendTextMessageAsync(id, "введите дополнительный комментарий к тесту[необязательно]", replyMarkup: NextAdCom);
            }
            else if (query.Data == "NextAdCom")
            {
                State[id].NumerPage = 0;
                State[id].ChatState = ChatState.SelectionDevice;
                await client.EditMessageReplyMarkupAsync(id, mesId);
                var devicesPage = await getDevicesPage.Get(new PageDto() { countElementsInPage = BotSettings.countElementsInPage, startPage = State[id].NumerPage });
                var btns = GetButtonsFromPage(State[id].NumerPage, devicesPage.ToList(), e => e.Name, e => e.Name);
                await client.SendTextMessageAsync(id, "аппарат:", replyMarkup: btns);
            }
            else if (query.Data == "AddNewUser")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].ChatState = ChatState.AddNewUser;
                var msg = await client.SendTextMessageAsync(id, "введите имя пользователя:", replyMarkup: backToSelectingUser);
                State[id].deleteButtons = new List<int>() { msg.MessageId };
            }
            else if (query.Data == "AddNewDevice")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].ChatState = ChatState.AddNewDevice;
                var msg = await client.SendTextMessageAsync(id, "введите название:", replyMarkup: backToSelectingDevice);
                State[id].deleteButtons = new List<int>() { msg.MessageId };
            }
            else if (query.Data == "SetRelease")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].ChatState = ChatState.Release;
                await client.SendTextMessageAsync(id, "версия:", replyMarkup: skiprelease);
            }
            else if (query.Data == "SkipRelease")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].ChatState = ChatState.AnswersTheQuestion;
                NextQuestion(client, id, State[id].QuestNumb);
            }
            else if (query.Data == "Tests")
            {
                State[id] = new();
                State[id].result.Answers = new List<Answer>();
                var tests = await getTestPage.Get(new PageDto { countElementsInPage = BotSettings.countElementsInPage, startPage = 0 });

                ViewTests(client, id, mesId, tests);
            }
            else if (query.Data == "Reports")
            {
                State[id].ChatState = ChatState.FirtsDate;
                await client.EditMessageReplyMarkupAsync(id, mesId);
                await client.SendTextMessageAsync(id, "начальная дата:", replyMarkup: skipfdate);
            }
            else if (query.Data == "SkipFirstDate")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].ChatState = ChatState.LastDate;
                await client.SendTextMessageAsync(id, "конечная дата:", replyMarkup: skipldate);
            }
            else if (query.Data == "SkipLastDate")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                var file = await excel.WriteResultsAsync(State[id].datesDto);
                if (file.Errors == null)
                {
                    using (Stream str = File.OpenRead(file.PathName))
                    {
                        await client.SendDocumentAsync(id, new(str, Path.GetFileName(file.PathName)));
                    }
                    State[id].ChatState = ChatState.None;
                    return;
                }
                await client.SendTextMessageAsync(id, string.Join('\n', file.Errors));
            }
            else if (query.Data == "BackToSelectingUser")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                var users = await getUsersPage.Get(new PageDto() { startPage = State[id].NumerPage, countElementsInPage = BotSettings.countElementsInPage });
                var buttons = GetButtonsFromPage(State[id].NumerPage, users.ToList(), e => e.Fio, e => e.TgId.ToString());
                await client.SendTextMessageAsync(id, "Выберите пользователя:", replyMarkup: buttons);
                State[id].ChatState = ChatState.SelectionUser;
            }
            else if (query.Data == "ChangeQuestions")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                var path = await getTestJson.Get(State[id].result.Test.Id);

                await SendAndDeleteDocument(client, id, path);
                State[id].ChatState = ChatState.ChangeTest;
            }
            else if (query.Data == "Login")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].result.Answers = new List<Answer>();
                for (int i = 0; i < State[id].Questions.Count(); i++)
                    State[id].result.Answers.Add(new Answer());

                var users = await getUsersPage.Get(new PageDto() { startPage = State[id].NumerPage, countElementsInPage = BotSettings.countElementsInPage });

                var buttons = GetButtonsFromPage(State[id].NumerPage, users.ToList(), e => e.Fio, e => e.TgId.ToString());

                await client.SendTextMessageAsync(id, "Выберите пользователя:", replyMarkup: buttons);
                State[id].ChatState = ChatState.SelectionUser;
            }
            else if (query.Data == "BackToSelectingDevice")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                var devicesPage = await getDevicesPage.Get(new PageDto() { startPage = State[id].NumerPage, countElementsInPage = BotSettings.countElementsInPage });
                var buttons = GetButtonsFromPage(State[id].NumerPage, devicesPage.ToList(), e => e.Name, e => e.Name);
                await client.SendTextMessageAsync(id, "Выберите аппарат:", replyMarkup: buttons);
                State[id].ChatState = ChatState.SelectionDevice;
            }
            else if (query.Data == "LastUserPage")
            {
                --State[id].NumerPage;
                var buttons = await getUsersPage.Get(new PageDto() { countElementsInPage = BotSettings.countElementsInPage, startPage = State[id].NumerPage });
                ChangePage(client, id, mesId, () => GetButtonsFromPage(State[id].NumerPage, buttons.ToList(), e => e.Fio, e => e.TgId.ToString()));
            }
            else if (query.Data == "NextTestPage")
            {
                ++State[id].NumerPage;
                var page = await getTestPage.Get(new PageDto { countElementsInPage = BotSettings.countElementsInPage, startPage = State[id].NumerPage });
                var buttons = GetButtonsFromPageToSelectTest(State[id].NumerPage, page.ToList(), e => e.Name, e => e.Id.ToString());
                ChangePage(client, id, mesId, () => buttons);
            }
            else if (query.Data == "LastTestPage")
            {
                --State[id].NumerPage;
                var page = await getTestPage.Get(new PageDto { countElementsInPage = BotSettings.countElementsInPage, startPage = State[id].NumerPage });
                var buttons = GetButtonsFromPageToSelectTest(State[id].NumerPage, page.ToList(), e => e.Name, e => e.Id.ToString());
                ChangePage(client, id, mesId, () => buttons);
            }
            else if (query.Data == "AddNewTest")
            {
                var test = new Test()
                {
                    Date = DateTime.Now,
                    Name = "имя(обязательно)",
                    Questions = new List<Question>
                    {
                        new Question{ },
                        new Question{ },
                    }
                };

                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented
                };
                await client.EditMessageReplyMarkupAsync(id, mesId);
                string json = JsonConvert.SerializeObject(test, settings);
                var path = "C:\\Users\\admin\\source\\repos\\TgBot\\TgBot\\bin\\Debug\\net5.0\\" + new Random().Next().ToString() + ".json";
                await File.WriteAllTextAsync(path, json);
                await client.SendTextMessageAsync(id, "заполните шаблон и отправьте мне");
                await SendAndDeleteDocument(client, id, path);
                State[id].ChatState = ChatState.AddTest;

            }
            else if (State[id].ChatState == ChatState.SelectingTest && ushort.TryParse(query?.Data, out testid))
            {
                State[id].NumerPage = 0;
                await CheckTest(client, id, testid, 0, mesId);
                var lastResult = await getLastresult.Get(testid);
#warning поправить (если было 2 или < то NA, а у меня проверка на null просто)
                if (lastResult is null)
                    await client.SendTextMessageAsync(id, $"Ver NA:");
                else
                {
                    string res = null;
                    res = await GetResult(lastResult.Answers);
                    await client.SendTextMessageAsync(id, $"Ver {lastResult.Version}: {res}");
                }
                var msg = await client.SendTextMessageAsync(id, "вы можете изменить тест или начать тестирование", replyMarkup: changeQuestionsAndLogin);
                State[id].deleteButtons = new List<int> { msg.MessageId };
            }
            else if (State[id].ChatState == ChatState.SelectingStoppedTest && int.TryParse(query.Data, out State[id].ResultId))
            {
                State[id].PassingStoppedTest = true;
                var testResult = await getTestResult.Get(State[id].ResultId);
                await CheckTest(client, id, testResult.Test.Id, testResult.PausedQuestionNumber.Value, mesId);
                State[id].result.Answers = testResult.Answers;
                State[id].ChatState = ChatState.AnswersTheQuestion;
                NextQuestion(client, id, State[id].QuestNumb);
            }
            else if (query.Data == "AddNewTestResult")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].ChatState = ChatState.CommentForTest;
                await client.SendTextMessageAsync(id, "введите комментарий к тесту[необязательно]", replyMarkup: NextCom);
            }
            else if (query.Data == "NextUserPage")
            {
                ++State[id].NumerPage;
                var buttons = await getUsersPage.Get(new PageDto() { countElementsInPage = BotSettings.countElementsInPage, startPage = State[id].NumerPage });
                ChangePage(client, id, mesId, () => GetButtonsFromPage(State[id].NumerPage, buttons.ToList(), e => e.Fio, e => e.TgId.ToString()));
            }
            else if (query.Data == "LastDevicePage")
            {
                --State[id].NumerPage;
                var devicesPage = await getDevicesPage.Get(new PageDto() { countElementsInPage = BotSettings.countElementsInPage, startPage = State[id].NumerPage });
                ChangePage(client, id, mesId, () => GetButtonsFromPage(State[id].NumerPage, devicesPage.ToList(), e => e.Name, e => e.Name));
            }
            else if (query.Data == "NextDevicePage")
            {
                ++State[id].NumerPage;
                var devicesPage = await getDevicesPage.Get(new PageDto() { countElementsInPage = BotSettings.countElementsInPage, startPage = State[id].NumerPage });
                ChangePage(client, id, mesId, () => GetButtonsFromPage(State[id].NumerPage, devicesPage.ToList(), e => e.Name, e => e.Name));
            }
            else if (State[id].ChatState == ChatState.SelectionUser && long.TryParse(query.Data, out State[id].result.UserId))
            {
                if (await HasStoppedTests(client, id, mesId)) return;

                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].ChatState = ChatState.CommentForTest;
                await client.SendTextMessageAsync(id, "введите комментарий к тесту[необязательно]", replyMarkup: NextCom);
            }
            else if (State[id].ChatState == ChatState.SelectionDevice)
            {
                State[id].result.Device = query.Data;
                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].ChatState = ChatState.Release;
                await client.SendTextMessageAsync(id, "версия:", replyMarkup: skiprelease);
            }
            else if (query.Data == "NextQuestion")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                if (!State[id].SkippedTestsFlag)
                {
                    State[id].QuestNumb++;
                    if (State[id].QuestNumb < State[id].Questions.Count())
                    {
                        State[id].ChatState = ChatState.AnswersTheQuestion; // <--- просто продолжение прохождение вопросов (не пропущенных)
                        NextQuestion(client, id, State[id].QuestNumb);
                        return;
                    }
                    State[id].QuestNumb = 0;

                    bool check1 = CheckSkippedQuestion(client, id, State[id].QuestNumb); // <---- возможно костыль \\ нужен для того, что бы вывод первого скипа работал нормально
                    if (check1) return; // тк без него вывод начинается со 2-го, а без инкремента, который чуть ниже, он зацикливается на 1-м
                }

                bool check2 = CheckSkippedQuestion(client, id, ++State[id].QuestNumb);
                if (check2) return;

                bool check3 = CheckSkippedQuestion(client, id, 0);// <---- проверка на то, что после прохождения скипов, у человек не нажимал снова скипы
                if (check3) return;

                SaveTestResult(client, id);
            }
        }
    }
}