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
using Domain.Dto;
using Domain.Model;
using TgBot;
using File = System.IO.File;
using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;
using System.Data;
using ExcelParser;

namespace TelegramBot
{
    internal partial class Program
    {
        static Dictionary<long, UserState> State;
        static BotSettings set = new BotSettings();

        static async Task Main(string[] args)
        {
            LoginInfo login = new LoginInfo();
            if (!File.Exists("login info.json"))
            {
                var s = new LoginInfo { login = "Mironova", password = "PlaTo2395" };

                login = s;

                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented
                };
                string json = JsonConvert.SerializeObject(s, settings);
                await File.WriteAllTextAsync("login info.json", json);
            }
            else
            {
                string jsonContent = File.ReadAllText("login info.json");

                var _login = JsonConvert.DeserializeObject<LoginInfo>(jsonContent);

                login = _login;
            }

            if (!File.Exists("настройки бота.json"))
            {
                await resetsettings("настройки бота.json");
            }
            else
            {
                string jsonContent = File.ReadAllText("настройки бота.json");

                var settings = JsonConvert.DeserializeObject<BotSettings>(jsonContent);

                set = settings;
            }

            ComponentInitialization();

            var proxy = new WebProxy
            {
                //Address = new Uri($"http://gw-srv.elektron.spb.su:3128"),
                //BypassProxyOnLocal = false,
                //UseDefaultCredentials = false,
                //Credentials = new NetworkCredential(
                //    userName: login.login,
                //    password: login.password)
            };
            var Httpclient = new HttpClient(handler: new HttpClientHandler { Proxy = proxy }, disposeHandler: true);
            var client = new TelegramBotClient(set.token, Httpclient);
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

        /// <summary>
        /// обработка текстовых сообщений и документов
        /// </summary>
        /// <returns></returns>
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
                await client.SendTextMessageAsync(id, "вы можете выбрать тест или получить отчеты", replyMarkup: start);
            }
            else if (message?.Text?.ToLower() == "/settings")
            {
                State[id].ChatState = ChatState.None;
                State[id].SkippedTestsFlag = false;

                var path = "настройки бота.json";

                if (!File.Exists(path))
                {
                    var settings = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        Formatting = Formatting.Indented
                    };
                    string json = JsonConvert.SerializeObject(set, settings);
                    await File.WriteAllTextAsync(path, json);
                }

                await SendDocument(client, id, path);
                await client.SendTextMessageAsync(id, "Измените этот файл и отправьте обратно мне");
                State[id].ChatState = ChatState.ChangeSettings;
            } // выдача файла с настройками (хранится там же где и бд), если файла нет, то создает новый с дефолтными параметрами
            else if (message?.Text?.ToLower() == "/resetsettings") // сбрасывает настройки до дефолтных значений и, если файла с ними нет, создает его
            {
                await client.SendTextMessageAsync(id, "сброс настроек");
                var path = "настройки бота.json";
                await resetsettings(path);
                await client.SendTextMessageAsync(id, "готово");
            }
            else if (State[id].ChatState == ChatState.ChangeSettings && message.Type == MessageType.Document)
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

                            var settings = JsonConvert.DeserializeObject<BotSettings>(jsonContent);

                            set = settings;

                            File.WriteAllText("настройки бота.json", jsonContent);
                        }
                        await client.SendTextMessageAsync(id, $"готово");
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(id, "не удалось запарсить файл\n" + ex.Message + '\n' + ex.InnerException?.Message);
                        await client.SendTextMessageAsync(id, "Проверьте файл и попробуйте еще раз");
                    }
                }

                State[id].ChatState = ChatState.None;
                await client.SendTextMessageAsync(id, "вы можете выбрать тест или получить отчеты", replyMarkup: start);
            }
            else if (State[id].ChatState == ChatState.AddNewUser)
            {
                await DeleteButtons(client, id);
                await addNewUser.Write(message.Text);
                var users = await getUsersPage.Get(new PageDto() { startPage = State[id].NumerPage, countElementsInPage = set.countElementsInPage });
                var buttons = GetButtonsFromPage(State[id].Role, State[id].NumerPage, users.ToList(), e => e.Fio, e => e.TgId.ToString());
                Message msg = await client.SendTextMessageAsync(id, "Выберите тестировщика", replyMarkup: buttons);
                State[id].deleteButtons = new int[] { msg.MessageId };
                State[id].ChatState = ChatState.SelectionUser;
            }
            else if (State[id].ChatState == ChatState.AddNewDevice)
            {
                await DeleteButtons(client, id);
                await addNewDevice.Write(message.Text);
                var devicesPage = await getDevicesPage.Get(new PageDto() { startPage = State[id].NumerPage, countElementsInPage = set.countElementsInPage });
                var buttons = GetButtonsFromPage(UserRole.Admin, State[id].NumerPage, devicesPage.ToList(), e => e.Name, e => e.Name);
                Message msg = await client.SendTextMessageAsync(id, "Выберите окружение", replyMarkup: buttons);
                State[id].deleteButtons = new int[] { msg.MessageId };
                State[id].ChatState = ChatState.SelectionDevice;
            }
            else if (State[id].ChatState == ChatState.Commenting)
            {
                await DeleteButtons(client, id);
                await client.SendTextMessageAsync(id, "Комментарий к проверке: " + message.Text);
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

                await EndTestingOrNextQuestion(client, id);
            }
            else if (State[id].ChatState == ChatState.AddProject)
            {
                await AddProject.Write(new Project { Name = message.Text });
                var groups = await getProjectPage.Get(new PageDto { countElementsInPage = set.countElementsInPage, startPage = 0 });
                await ViewProjects(client, id, groups);
            }
            else if (State[id].ChatState == ChatState.FirtsDate)
            {
                await DeleteButtons(client, id);
                var date = ParseDate(message.Text);
                if (date.HasErrors)
                {
                    await client.SendTextMessageAsync(id, $"Что-то пошло не так. Попробуйте снова\n{string.Join('\n', date.Errors.Select(p => p.ErrorResult.ErrorMessage))}");
                    return;
                }
                await client.SendTextMessageAsync(id, "Введенная начальная дата: " + message.Text);

                State[id].datesDto.fdate = date.Result;
                State[id].ChatState = ChatState.LastDate;
                var msg = await client.SendTextMessageAsync(id, "конечная дата:", replyMarkup: skipldate);
                State[id].deleteButtons = new int[] { msg.MessageId };
            }
            else if (State[id].ChatState == ChatState.LastDate)
            {
                await DeleteButtons(client, id);
                var date = ParseDate(message.Text);
                if (date.HasErrors)
                {
                    await client.SendTextMessageAsync(id, $"Что-то пошло не так. Попробуйте снова\n{string.Join('\n', date.Errors.Select(p => p.ErrorResult.ErrorMessage))}");
                    return;
                }
                State[id].datesDto.ldate = date.Result;
                await client.SendTextMessageAsync(id, "Введенная конечная дата: " + message.Text);
                var file = await excel.WriteResultsAsync(State[id].datesDto);
                if (file.Errors == null)
                {
                    await SendAndDeleteDocument(client, id, file.PathName);
                    State[id].ChatState = ChatState.None;
                    return;
                }
                await client.SendTextMessageAsync(id, string.Join('\n', file.Errors));
            }
            else if (State[id].ChatState == ChatState.CommentForTest)
            {
                await DeleteButtons(client, id);
                State[id].result.CommentFromTest = message.Text;
                State[id].ChatState = ChatState.AdditionalCommentForTest;
                await client.SendTextMessageAsync(id, "Комментарий к тесту: " + message.Text);
                Message msg = await client.SendTextMessageAsync(id, "введите дополнительный комментарий к тесту[необязательно]", replyMarkup: NextAdCom);
                State[id].deleteButtons = new[] { msg.MessageId };
            }
            else if (State[id].ChatState == ChatState.AdditionalCommentForTest)
            {
                await DeleteButtons(client, id);
                await client.SendTextMessageAsync(id, "Дополнительный комментарий к тесту: " + message.Text);
                State[id].result.AdditionalCommentForTest = message.Text;
                State[id].NumerPage = 0;
                State[id].ChatState = ChatState.SelectionDevice;
                var devicesPage = await getDevicesPage.Get(new PageDto() { countElementsInPage = set.countElementsInPage, startPage = State[id].NumerPage });
                var btns = GetButtonsFromPage(UserRole.Admin, State[id].NumerPage, devicesPage.ToList(), e => e.Name, e => e.Name);
                Message msg = await client.SendTextMessageAsync(id, "Выберите окружение", replyMarkup: btns);
                State[id].deleteButtons = new[] { msg.MessageId };
            }
            else if (State[id].ChatState == ChatState.Release)
            {
                await client.SendTextMessageAsync(id, "Введенная сборка: " + message.Text);
                State[id].result.Version = message.Text;
                State[id].ChatState = ChatState.AnswersTheQuestion;
                await SendQuestion(client, id, State[id].QuestNumb);
            }
            else if (State[id].ChatState == ChatState.AddTest && message.Type == MessageType.Document)
            {
                var file = await client.GetFileAsync(message.Document.FileId);
                
                using (var stream = new MemoryStream())
                {
                    string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, message.Document.FileName);
                    Test test = null;

                    try
                    {
                        using (var fileStream = new FileStream(filepath, FileMode.Create))
                        {
                            await client.DownloadFileAsync(file.FilePath, fileStream);
                        }
                        FileInfo finfo = new FileInfo(filepath);

                        if (finfo.Extension == ".xls" || finfo.Extension == ".xlsx")
                        {
                            test = SitichkoExcelParser.Parser(filepath);
                        }
                        else
                        {
                            stream.Position = 0;
                            using (var reader = new StreamReader(stream))
                            {
                                string jsonContent = await reader.ReadToEndAsync();

                                test = JsonConvert.DeserializeObject<Test>(jsonContent);

                            }
                        }
                        File.Delete(filepath);
                        test.Project = new Project { Id = State[id].ProjectId };

                        await AddTest.Write(test);
                        await client.SendTextMessageAsync(id, $"готово");
                        State[id].result.Answers = new List<Answer>();
                        var tests = await getTestPage.Get(new TestPageDto { pageSet = new PageDto { countElementsInPage = set.countElementsInPage, startPage = 0 }, projectId = State[id].ProjectId });
                        await ViewTests(client, id, tests);
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(id, "не удалось запарсить файл\n" + ex.Message + '\n' + ex.InnerException?.Message);
                        await client.SendTextMessageAsync(id, "Проверьте файл и попробуйте еще раз");
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
                        var tests = await getTestPage.Get(new TestPageDto { pageSet = new PageDto { countElementsInPage = set.countElementsInPage, startPage = 0 }, projectId = State[id].ProjectId });
                        await ViewTests(client, id, tests);
                    }
                    catch (ArgumentException ex)
                    {
                        await client.SendTextMessageAsync(id, "не удалось записать изменения\n" + ex.Message);
                        await client.SendTextMessageAsync(id, "проверьте файл и попробуйте еще раз");
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(id, $"не удалось запарсить файл\n" + ex.Message + '\n' + ex.InnerException?.Message);
                        await client.SendTextMessageAsync(id, "проверьте файл и попробуйте еще раз");
                    }
                }

            }
        }

        /// <summary>
        /// обработка нажатия кнопок
        /// </summary>
        /// <returns></returns>
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
                    await StopTesting(id, State[id].QuestNumb);
                    await client.SendTextMessageAsync(id, "тест был остановлен");
                    var tests = await getTestPage.Get(new TestPageDto { pageSet = new PageDto { countElementsInPage = set.countElementsInPage, startPage = 0 }, projectId = State[id].ProjectId });
                    await client.EditMessageReplyMarkupAsync(id, mesId);
                    await ViewTests(client, id, tests);
                    return;
                }
                else if (query.Data == "LATER")
                {
                    await client.EditMessageReplyMarkupAsync(id, mesId);
                    await client.SendTextMessageAsync(id, "Вы вернетесь к этой проверке в конце теста");
                    State[id].result.Answers[State[id].QuestNumb].Result = query.Data;
                    await EndTestingOrNextQuestion(client, id);
                    return;
                }
                else if (query.Data == "WriteLastResult")
                {
                    InlineKeyboardMarkup buttons = GetButtonsFromAnwer(id);

                    State[id].ChatState = ChatState.WriteLastResult;

                    await client.EditMessageReplyMarkupAsync(id, mesId, replyMarkup: buttons);
                    return;
                }

                await client.EditMessageReplyMarkupAsync(id, mesId);
                await client.SendTextMessageAsync(id, "выбранный результат проверки: " + query.Data);
                State[id].ChatState = ChatState.Commenting;
                State[id].result.Answers[State[id].QuestNumb].Result = query.Data;
                Message msg;
                if (query.Data == "PASS")
                {
                    msg = await client.SendTextMessageAsync(id, "Введите комментарий[необязательно]", replyMarkup: next);
                    State[id].deleteButtons = new[] { msg.MessageId };
                }
                else
                    await client.SendTextMessageAsync(id, "Введите комментарий");
            }
            else if (query.Data == "NextCom")
            {
                State[id].deleteButtons = null;
                State[id].ChatState = ChatState.AdditionalCommentForTest;
                await client.EditMessageReplyMarkupAsync(id, mesId);
                await client.SendTextMessageAsync(id, "Дополнительный комментарий к тесту: пусто");
                Message msg = await client.SendTextMessageAsync(id, "введите дополнительный комментарий к тесту[необязательно]", replyMarkup: NextAdCom);
                State[id].deleteButtons = new int[] {msg.MessageId};
            }
            else if (query.Data == "NextAdCom")
            {
                State[id].deleteButtons = null;
                State[id].NumerPage = 0;
                State[id].ChatState = ChatState.SelectionDevice;
                await client.EditMessageReplyMarkupAsync(id, mesId);
                await client.SendTextMessageAsync(id, "Дополнительный комментарий к тесту: пусто");
                var devicesPage = await getDevicesPage.Get(new PageDto() { countElementsInPage = set.countElementsInPage, startPage = State[id].NumerPage });
                var btns = GetButtonsFromPage(UserRole.Admin, State[id].NumerPage, devicesPage.ToList(), e => e.Name, e => e.Name);
                await client.SendTextMessageAsync(id, "Выберите окружение:", replyMarkup: btns);
            }
            else if (query.Data == "RoleNone" || query.Data == "RoleAdmin")
            {
                string role = "";
                if (query.Data == "RoleAdmin")
                {
                    State[id].Role = UserRole.Admin;
                    role = "Админ";
                }
                else
                {
                    State[id].Role = UserRole.None;
                    role = "Обычный пользователь";
                }

                await client.EditMessageReplyMarkupAsync(id, mesId);
                await client.SendTextMessageAsync(id, "Ваша роль: " + role);

                var groups = await getProjectPage.Get(new PageDto { countElementsInPage = set.countElementsInPage, startPage = 0 });
                await ViewProjects(client, id, groups);
            }
            else if (query.Data == "Tests")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id] = new();
                State[id].result.Answers = new List<Answer>();
                await client.SendTextMessageAsync(id, "выберите роль", replyMarkup: roles);
            }
            else if (query.Data == "Reports")
            {
                var groups = await getProjectPage.Get(new PageDto { countElementsInPage = set.countElementsInPage, startPage = 0 });
                await ViewProjectsForReport(client, id, groups);

                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].ChatState = ChatState.SelectingProjectForReport;
            }
            else if (query.Data == "SkipFirstDate")
            {
                await DeleteButtons(client, id);
                await client.SendTextMessageAsync(id, "Введенная начальная дата: пусто");
                State[id].ChatState = ChatState.LastDate;
                var msg = await client.SendTextMessageAsync(id, "конечная дата:", replyMarkup: skipldate);
                State[id].deleteButtons = new int[] { msg.MessageId };
            }
            else if (query.Data == "SkipLastDate")
            {
                await DeleteButtons(client, id);
                await client.SendTextMessageAsync(id, "Введенная конечная дата: пусто");
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
                var users = await getUsersPage.Get(new PageDto() { startPage = State[id].NumerPage, countElementsInPage = set.countElementsInPage });
                var buttons = GetButtonsFromPage(State[id].Role, State[id].NumerPage, users.ToList(), e => e.Fio, e => e.TgId.ToString());
                await client.SendTextMessageAsync(id, "Выберите тестировщика", replyMarkup: buttons);
                State[id].ChatState = ChatState.SelectionUser;
            }
            else if (query.Data == "ChangeQuestions")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                var test = await getTest.Get(State[id].result.Test.Id);
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented
                };
                var now = DateTime.Now;
                string json = await Task.Run(() => JsonConvert.SerializeObject(test, settings));
                var path = test.Name + ' ' + DateTime.Now.ToShortDateString() + ".json";
                await File.WriteAllTextAsync(path, json);

                await SendAndDeleteDocument(client, id, path);
                State[id].ChatState = ChatState.ChangeTest;
            }
            else if (query.Data == "Login")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].result.Answers = new List<Answer>();
                for (int i = 0; i < State[id].Questions.Count(); i++)
                    State[id].result.Answers.Add(new Answer());

                var users = await getUsersPage.Get(new PageDto() { startPage = State[id].NumerPage, countElementsInPage = set.countElementsInPage });

                var buttons = GetButtonsFromPage(State[id].Role, State[id].NumerPage, users.ToList(), e => e.Fio, e => e.TgId.ToString());

                await client.SendTextMessageAsync(id, "Выберите тестировщика", replyMarkup: buttons);
                State[id].ChatState = ChatState.SelectionUser;
            }
            else if (query.Data == "BackToSelectingDevice")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                var devicesPage = await getDevicesPage.Get(new PageDto() { startPage = State[id].NumerPage, countElementsInPage = set.countElementsInPage });
                var buttons = GetButtonsFromPage(UserRole.Admin, State[id].NumerPage, devicesPage.ToList(), e => e.Name, e => e.Name);
                await client.SendTextMessageAsync(id, "Выберите окружение", replyMarkup: buttons);
                State[id].ChatState = ChatState.SelectionDevice;
            }
            else if (query.Data == "SendQuestion")
            {
                State[id].deleteButtons = null;
                await client.EditMessageReplyMarkupAsync(id, mesId);
                await client.SendTextMessageAsync(id, "Комментарий к проверке: пусто");
                await EndTestingOrNextQuestion(client, id);
            }
            else if (query.Data == "WriteLastResult_BackButton")
            {
                State[id].ChatState = ChatState.AnswersTheQuestion;
                await client.EditMessageReplyMarkupAsync(id, mesId, replyMarkup: answer);
            }
            #region добавление чего-то нового
            else if (query.Data == "AddNewUser")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].ChatState = ChatState.AddNewUser;
                var msg = await client.SendTextMessageAsync(id, "введите имя пользователя:", replyMarkup: backToSelectingUser);
                State[id].deleteButtons = new int[] { msg.MessageId };
            }
            else if (query.Data == "AddNewDevice")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].ChatState = ChatState.AddNewDevice;
                var msg = await client.SendTextMessageAsync(id, "введите название:", replyMarkup: backToSelectingDevice);
                State[id].deleteButtons = new int[] { msg.MessageId };
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
                var path = "шаблон" + DateTime.Now.Millisecond + ".json";
                await File.WriteAllTextAsync(path, json);
                await client.SendTextMessageAsync(id, "заполните шаблон и отправьте мне ИЛИ отправьте xls файл с тестом с сайта https://sitechco.ru/");
                await SendAndDeleteDocument(client, id, path);
                State[id].ChatState = ChatState.AddTest;

            }
            else if (query.Data == "AddNewProject")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                await client.SendTextMessageAsync(id, "Введите название проекта");
                State[id].ChatState = ChatState.AddProject;
            }
            else if (query.Data == "AddNewTestResult")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                await client.SendTextMessageAsync(id, "Вы начали новое тестирование");
                State[id].ChatState = ChatState.CommentForTest;
                Message msg = await client.SendTextMessageAsync(id, "введите комментарий к тесту[необязательно]", replyMarkup: NextCom);
                State[id].deleteButtons = new int[] { msg.MessageId };
            }
            #endregion добавление чего-то нового
            #region Переключение страниц
            else if (query.Data == "LastDevicePage")
            {
                --State[id].NumerPage;
                var devicesPage = await getDevicesPage.Get(new PageDto() { countElementsInPage = set.countElementsInPage, startPage = State[id].NumerPage });
                await ChangePage(client, id, mesId, () => GetButtonsFromPage(UserRole.Admin, State[id].NumerPage, devicesPage.ToList(), e => e.Name, e => e.Name));
            }
            else if (query.Data == "NextDevicePage")
            {
                ++State[id].NumerPage;
                var devicesPage = await getDevicesPage.Get(new PageDto() { countElementsInPage = set.countElementsInPage, startPage = State[id].NumerPage });
                await ChangePage(client, id, mesId, () => GetButtonsFromPage(UserRole.Admin, State[id].NumerPage, devicesPage.ToList(), e => e.Name, e => e.Name));
            }
            else if (query.Data == "NextTestResultPage")
            {
                ++State[id].NumerPage;
                var stoppedTestsPage = await getStoppedTestsPage.Get(new StoppedTestResultPageDto
                {
                    pageSet = new PageDto() { countElementsInPage = set.countElementsInPage, startPage = State[id].NumerPage },
                    userId = State[id].result.UserId,
                    testId = State[id].result.Test.Id
                });
                await ChangePage(client, id, mesId, () => GetButtonsFromPageToContinueTesting(UserRole.Admin, State[id].NumerPage, stoppedTestsPage.ToList(), e => e.Date.ToShortDateString() + ' ' + e.Date.ToShortTimeString(), e => e.Id.ToString()));
            }
            else if (query.Data == "LastTestResultPage")
            {
                --State[id].NumerPage;
                var stoppedTestsPage = await getStoppedTestsPage.Get(new StoppedTestResultPageDto
                {
                    pageSet = new PageDto() { countElementsInPage = set.countElementsInPage, startPage = State[id].NumerPage },
                    userId = State[id].result.UserId,
                    testId = State[id].result.Test.Id
                });
                await ChangePage(client, id, mesId, () => GetButtonsFromPageToContinueTesting(UserRole.Admin, State[id].NumerPage, stoppedTestsPage.ToList(), e => e.Date.ToShortDateString() + ' ' + e.Date.ToShortTimeString(), e => e.Id.ToString()));
            }
            else if (query.Data == "LastUserPage")
            {
                --State[id].NumerPage;
                var buttons = await getUsersPage.Get(new PageDto() { countElementsInPage = set.countElementsInPage, startPage = State[id].NumerPage });
                await ChangePage(client, id, mesId, () => GetButtonsFromPage(State[id].Role, State[id].NumerPage, buttons.ToList(), e => e.Fio, e => e.TgId.ToString()));
            }
            else if (query.Data == "NextUserPage")
            {
                ++State[id].NumerPage;
                var buttons = await getUsersPage.Get(new PageDto() { countElementsInPage = set.countElementsInPage, startPage = State[id].NumerPage });
                await ChangePage(client, id, mesId, () => GetButtonsFromPage(State[id].Role, State[id].NumerPage, buttons.ToList(), e => e.Fio, e => e.TgId.ToString()));
            }
            else if (query.Data == "NextTestPage")
            {
                InlineKeyboardMarkup buttons = null;
                ++State[id].NumerPage;
                var page = await getTestPage.Get(new TestPageDto { pageSet = new PageDto { countElementsInPage = set.countElementsInPage, startPage = State[id].NumerPage }, projectId = State[id].ProjectId });

                //нужно для того, чтобы кнопки добавить в отчетах не было
                if (State[id].ChatState != ChatState.SelectingTestToReports)
                    buttons = GetButtonsFromPageToSelectTest(State[id].Role, State[id].NumerPage, page.ToList(), e => $"{e.Name}", e => e.Id.ToString());
                else
                    buttons = GetButtonsFromPageToReports(State[id].NumerPage, page.ToList(), e => $"{e.Name}", e => e.Id.ToString());
                await ChangePage(client, id, mesId, () => buttons);
            }
            else if (query.Data == "LastTestPage")
            {
                --State[id].NumerPage;
                InlineKeyboardMarkup buttons = null;
                var page = await getTestPage.Get(new TestPageDto { pageSet = new PageDto { countElementsInPage = set.countElementsInPage, startPage = State[id].NumerPage }, projectId = State[id].ProjectId });

                //нужно для того, чтобы кнопки добавить в отчетах не было
                if (State[id].ChatState != ChatState.SelectingTestToReports)
                    buttons = GetButtonsFromPageToSelectTest(State[id].Role, State[id].NumerPage, page.ToList(), e => $"{e.Name}", e => e.Id.ToString());
                else
                    buttons = GetButtonsFromPageToReports(State[id].NumerPage, page.ToList(), e => $"{e.Name}", e => e.Id.ToString());
                await ChangePage(client, id, mesId, () => buttons);
            }
            else if (query.Data == "NextProjectPage")
            {
                ++State[id].NumerPage;
                InlineKeyboardMarkup buttons = null;
                var page = await getProjectPage.Get(new PageDto() { countElementsInPage = set.countElementsInPage, startPage = State[id].NumerPage });

                //нужно для того, чтобы кнопки добавить в отчетах не было
                if (State[id].ChatState != ChatState.SelectingProjectForReport)
                    buttons = GetButtonsFromPageToSelectTest(State[id].Role, State[id].NumerPage, page.ToList(), e => $"{e.Name}", e => e.Id.ToString());
                else
                    buttons = GetButtonsFromPageToReports(State[id].NumerPage, page.ToList(), e => $"{e.Name}", e => e.Id.ToString());

                await ChangePage(client, id, mesId, () => buttons);
            }
            else if (query.Data == "LastProjectPage")
            {
                --State[id].NumerPage;
                InlineKeyboardMarkup buttons = null;
                var page = await getProjectPage.Get(new PageDto() { countElementsInPage = set.countElementsInPage, startPage = State[id].NumerPage });

                //нужно для того, чтобы кнопки добавить в отчетах не было
                if (State[id].ChatState != ChatState.SelectingProjectForReport)
                    buttons = GetButtonsFromPageToSelectTest(State[id].Role, State[id].NumerPage, page.ToList(), e => $"{e.Name}", e => e.Id.ToString());
                else
                    buttons = GetButtonsFromPageToReports(State[id].NumerPage, page.ToList(), e => $"{e.Name}", e => e.Id.ToString());

                await ChangePage(client, id, mesId, () => buttons);
            }
            #endregion Переключение страниц
            else if (State[id].ChatState == ChatState.SelectionUser && long.TryParse(query.Data, out State[id].result.UserId))
            {
                var buttons = query.Message.ReplyMarkup.InlineKeyboard;

                foreach (var hei in buttons)
                {
                    foreach (var width in hei)
                    {
                        if (query.Data == width.CallbackData)
                            State[id].result.UserName = width.Text;
                    }
                }

                await client.SendTextMessageAsync(id, "Выбранный тестировщик: " + State[id].result.UserName);

                if (await HasStoppedTests(client, id, mesId)) return;

                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].ChatState = ChatState.CommentForTest;
                Message msg = await client.SendTextMessageAsync(id, "введите комментарий к тесту[необязательно]", replyMarkup: NextCom);
                State[id].deleteButtons = new[] { msg.MessageId };
            }
            else if (State[id].ChatState == ChatState.SelectionDevice)
            {
                State[id].deleteButtons = null;
                State[id].result.Device = query.Data;
                await client.EditMessageReplyMarkupAsync(id, mesId);
                await client.SendTextMessageAsync(id, "Выбранное окружение: " + query.Data);
                State[id].ChatState = ChatState.Release;
                await client.SendTextMessageAsync(id, "Введите сборку");
            }
            else if (State[id].ChatState == ChatState.GetReport && sbyte.TryParse(query.Data, out State[id].datesDto.variant))
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);

                string variant = "";

                foreach (var hei in query.Message.ReplyMarkup.InlineKeyboard)
                {
                    foreach (var width in hei)
                    {
                        if (query.Data == width.CallbackData)
                            variant = width.Text;
                    }
                }

                await client.SendTextMessageAsync(id, "Ваш выбор: " + variant);

                if (State[id].datesDto.variant == 4)
                {
                    State[id].ChatState = ChatState.FirtsDate;
                    var msg = await client.SendTextMessageAsync(id, "начальная дата:", replyMarkup: skipfdate);
                    State[id].deleteButtons = new int[]{ msg.MessageId };
                    return;
                }

                var file = await excel.WriteResultsAsync(State[id].datesDto);
                if (file.Errors == null)
                {
                    await SendAndDeleteDocument(client, id, file.PathName);
                    State[id].ChatState = ChatState.None;
                    return;
                }
                await client.SendTextMessageAsync(id, string.Join('\n', file.Errors));
            }
            else if (State[id].ChatState == ChatState.SelectingProject && ushort.TryParse(query?.Data, out ushort projectid))
            {
                string variant = "";

                foreach (var hei in query.Message.ReplyMarkup.InlineKeyboard)
                {
                    foreach (var width in hei)
                    {
                        if (query.Data == width.CallbackData)
                            variant = width.Text;
                    }
                }

                State[id].NumerPage = 0;
                await client.EditMessageReplyMarkupAsync(id, mesId);
                await client.SendTextMessageAsync(id, "Выбранный проект: " + variant);
                State[id].ProjectId = projectid;
                var tests = await getTestPage.Get(new TestPageDto { pageSet = new PageDto { countElementsInPage = set.countElementsInPage, startPage = 0 }, projectId = projectid });
                await ViewTests(client, id, tests);
            }
            else if (State[id].ChatState == ChatState.SelectingProjectForReport && ushort.TryParse(query?.Data, out State[id].ProjectId))
            {
                string variant = "";

                foreach (var hei in query.Message.ReplyMarkup.InlineKeyboard)
                {
                    foreach (var width in hei)
                    {
                        if (query.Data == width.CallbackData)
                            variant = width.Text;
                    }
                }

                State[id].NumerPage = 0;
                await client.EditMessageReplyMarkupAsync(id, mesId);
                await client.SendTextMessageAsync(id, "Выбранный проект: " + variant);
                var tests = await getTestPage.Get(new TestPageDto { pageSet = new PageDto { countElementsInPage = set.countElementsInPage, startPage = 0 }, projectId = State[id].ProjectId });
                await ViewTestsForReports(client, id, tests);
                State[id].ChatState = ChatState.SelectingTestToReports;
            }
            else if (State[id].ChatState == ChatState.SelectingTest && ushort.TryParse(query?.Data, out testid))
            {
                State[id].NumerPage = 0;
                await CheckTest(client, id, testid, 0, mesId);
                await client.SendTextMessageAsync(id, "Выбранный тест: " + State[id].result.Test?.Name);
                var lastResult = await getLastresult.Get(testid);

                if (lastResult != null)
                {
                    int answerCount = lastResult.Answers.Count();
                    int passCount = lastResult.Answers.Where(e => e.Result == "PASS").Count();

                    await client.SendTextMessageAsync(id, $"Данные о прошлом прохождении.\nСборка: {lastResult.Version}\nОкружение: {lastResult.Apparat}\nРезультат: {passCount}/{answerCount}");
                }
                else
                {
                    await client.SendTextMessageAsync(id, $"Данных о прошлом прохождении нет");
                }

                Message msg = null;
                if (State[id].Role == UserRole.Admin)
                    msg = await client.SendTextMessageAsync(id, "вы можете изменить тест или начать тестирование", replyMarkup: changeQuestionsAndLogin);
                else if (State[id].Role == UserRole.None)
                {
                    State[id].result.Answers = new List<Answer>();
                    for (int i = 0; i < State[id].Questions.Count(); i++)
                        State[id].result.Answers.Add(new Answer());

                    var users = await getUsersPage.Get(new PageDto() { startPage = State[id].NumerPage, countElementsInPage = set.countElementsInPage });

                    var buttons = GetButtonsFromPage(State[id].Role, State[id].NumerPage, users.ToList(), e => e.Fio, e => e.TgId.ToString());

                    await client.SendTextMessageAsync(id, "Выберите тестировщика", replyMarkup: buttons);
                    State[id].ChatState = ChatState.SelectionUser;
                }
            }
            else if (State[id].ChatState == ChatState.SelectingTestToReports && ushort.TryParse(query?.Data, out testid))
            {
                State[id].NumerPage = 0;
                var test = await getTest.Get(testid);
                await client.SendTextMessageAsync(id, "Выбранный тест: " + test.Name);
                State[id].datesDto.TestId = testid;
                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].ChatState = ChatState.GetReport;
                await client.SendTextMessageAsync(id, "Что вы хотите получить?", replyMarkup: selectVariantReport);
            }
            else if (State[id].ChatState == ChatState.SelectingStoppedTest && int.TryParse(query.Data, out State[id].ResultId))
            {
                State[id].isPassingStoppedTest = true;

                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].SkippedTestsFlag = true; //edit
                var testResult = await getTestResult.Get(State[id].ResultId);
                await client.SendTextMessageAsync(id, "Вы продолжили тестирование, остановленное " + testResult.Date.ToShortDateString() + " в " + testResult.Date.ToShortTimeString());
                State[id].result.Answers = testResult.Answers;
                State[id].ChatState = ChatState.AnswersTheQuestion;

                if (testResult.PausedQuestionNumber == null)
                {
                    await client.SendTextMessageAsync(id, "по каким-то причинам номер проверки, на которой остановилось тестирование, было null ");
                    return;
                }
                State[id].QuestNumb = testResult.PausedQuestionNumber.Value;

                await SendQuestion(client, id, testResult.PausedQuestionNumber.Value);
            }
            else if (State[id].ChatState == ChatState.WriteLastResult && sbyte.TryParse(query.Data, out sbyte lastResultNumber))
            {
                await client.SendTextMessageAsync(id, "Вставлен результат одного из прошлых ответов.\nРезультат: " + State[id].LastAnswers[lastResultNumber].Result + "\nКомментарий: " + State[id].LastAnswers[lastResultNumber].Comment);

                State[id].result.Answers[State[id].QuestNumb].Result = State[id].LastAnswers[lastResultNumber].Result;
                State[id].result.Answers[State[id].QuestNumb].Comment = State[id].LastAnswers[lastResultNumber].Comment;

                await client.EditMessageReplyMarkupAsync(id, mesId);

                State[id].ChatState = ChatState.AnswersTheQuestion;

                await EndTestingOrNextQuestion(client, id);
            }
        }
    }
}