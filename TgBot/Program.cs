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
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;

namespace TelegramBot
{
    internal partial class Program
    {
        const string token = "6185570726:AAHBPUqL-qMSrmod9YxV6ot3IKrJ3YXzzCc";
        static string[] _commands = {"/start"};
        static Dictionary<long, UserState> State;

        static IExcelGenerator<Task<FileDto>, DatesForExcelDTO> excel;
        static IGetCommand<Test, ushort> getTest;
        static IGetCommand<IList<Test>> getSortedTests;
        static IGetCommand<IEnumerable<Domain.Model.User>, PageDto> getUsersPage;
        static IGetCommand<IEnumerable<Device>, PageDto> getDevicesPage;
        static IWriteCommand<IReadOnlyList<ValidationResult>, ResultTestDto> saveResult;
        static IWriteCommand<string> addNewUser;
        static IWriteCommand<string> addNewDevice;

        static void Main(string[] args)
        {
            ComponentInitialization();
            ScopedComponentInitialization();

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
            var client = new TelegramBotClient(token, Httpclient);
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

        private static Task Error(ITelegramBotClient client, Exception ex, CancellationToken token)
        {
            Console.WriteLine($"{ex.GetType()}\n{ex.Message}\n{ex.InnerException.Message}");
            throw new Exception(ex.Message + '\n' + '\n' + ex.InnerException.Message);
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
            else if (message?.Text?.ToLower() == "/commands")
            {
                await client.SendTextMessageAsync(id, $"{string.Join('\n', _commands)}");
                return;
            }

            ushort testid;
            if (ushort.TryParse(message?.Text, out testid) && State[id].ChatState == ChatState.SelectingTest)
            {
                var test = getTest.Get(testid);
                if (test == null)
                {
                    await client.SendTextMessageAsync(id, "вы выбрали не существующий тест");
                    return;
                }

                if (test.Questions.Count() == 0)
                {
                    await client.SendTextMessageAsync(id, "в тесте нет вопросов.\nвы можете выбрать другой тест");
                    return;
                }

                State[id].Questions = test.Questions;
                State[id].result.Test = test;
                State[id].QuestNumb = 0;

                State[id].result.Answers = new List<Answer>();
                for (int i = 0; i < State[id].Questions.Count(); i++)
                    State[id].result.Answers.Add(new Answer());

                var users = getUsersPage.Get(new PageDto() { startPage = State[id].NumerPage, countElementsInPage = BotSettings.countElementsInPage });

                var buttons = GetButtonsFromPage(State[id].NumerPage, users.ToList(), e => e.Fio, e => e.Fio.ToString());

                await client.SendTextMessageAsync(id, "Выберите пользователя:", replyMarkup: buttons);
                State[id].ChatState = ChatState.SelectionUser;
            }
            else if (State[id].ChatState == ChatState.SetFio)
            {
                State[id].result.UserName = message.Text;
                await client.SendTextMessageAsync(id, "введите комментарий к тесту[необязательно]", replyMarkup: NextCom);
                State[id].ChatState = ChatState.CommentForTest;
            }
            else if (State[id].ChatState == ChatState.AddNewUser)
            {
                DeleteButtons(client, id);
                addNewUser.Write(message.Text);
                var users = getUsersPage.Get(new PageDto() { startPage = State[id].NumerPage, countElementsInPage = BotSettings.countElementsInPage });
                var buttons = GetButtonsFromPage(State[id].NumerPage, users.ToList(), e => e.Fio, e => e.Fio.ToString());
                Message msg = await client.SendTextMessageAsync(id, "Выберите пользователя:", replyMarkup: buttons);
                State[id].deleteButtons = new List<int>() { msg.MessageId };
                State[id].ChatState = ChatState.SelectionUser;
            }
            else if (State[id].ChatState == ChatState.AddNewDevice)
            {
                DeleteButtons(client, id);
                addNewDevice.Write(message.Text);
                var devicesPage = getDevicesPage.Get(new PageDto() { startPage = State[id].NumerPage, countElementsInPage = BotSettings.countElementsInPage });
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
                    using (Stream str = System.IO.File.OpenRead(file.PathName))
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
        }

        async static Task HandleCallBackQuery(ITelegramBotClient client, CallbackQuery query)
        {
            var id = query.Message.Chat.Id;
            if (!State.ContainsKey(id)) return;
            var mesId = query.Message.MessageId;

            Console.WriteLine($"{query.Message.Date} {query.From.Username} {query.From.Id}: {query.Data}");

            if (State[id].ChatState == ChatState.AnswersTheQuestion)
            {
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
                var devicesPage = getDevicesPage.Get(new PageDto() { countElementsInPage = BotSettings.countElementsInPage, startPage = State[id].NumerPage });
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
                var tests = getSortedTests.Get();

                if (tests.Count() == 0)
                {
                    await client.EditMessageTextAsync(id, mesId, "тестов нет");
                    return;
                }
                else
                {
                    var testNamesAndDate = "id Name     Date\n" + string.Join("\n", tests.Select(p => $"{p.Id}  {p.Name}   {p.Date.ToShortDateString()}"));
                    await client.EditMessageTextAsync(id, mesId, testNamesAndDate);
                    await client.SendTextMessageAsync(id, "выберете тест и напишите его id");
                    State[id].ChatState = ChatState.SelectingTest;
                }
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
                    using (Stream str = System.IO.File.OpenRead(file.PathName))
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
                var users = getUsersPage.Get(new PageDto() { startPage = State[id].NumerPage, countElementsInPage = BotSettings.countElementsInPage });
                var buttons = GetButtonsFromPage(State[id].NumerPage, users.ToList(), e => e.Fio, e => e.TgId.ToString());
                await client.SendTextMessageAsync(id, "Выберите пользователя:", replyMarkup: buttons);
                State[id].ChatState = ChatState.SelectionUser;
            }
            else if (query.Data == "BackToSelectingDevice")
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                var devicesPage = getDevicesPage.Get(new PageDto() { startPage = State[id].NumerPage, countElementsInPage = BotSettings.countElementsInPage });
                var buttons = GetButtonsFromPage(State[id].NumerPage, devicesPage.ToList(), e => e.Name, e => e.Name);
                await client.SendTextMessageAsync(id, "Выберите аппарат:", replyMarkup: buttons);
                State[id].ChatState = ChatState.SelectionDevice;
            }
            else if (query.Data == "LastUserPage")
            {
                --State[id].NumerPage;
                var buttons = getUsersPage.Get(new PageDto() { countElementsInPage = BotSettings.countElementsInPage, startPage = State[id].NumerPage }).ToList();
                ChangePage(client, id, mesId, () => GetButtonsFromPage(State[id].NumerPage, buttons, e => e.Fio, e => e.TgId.ToString()));
            }
            else if (query.Data == "NextUserPage")
            {
                ++State[id].NumerPage;
                var buttons = getUsersPage.Get(new PageDto() { countElementsInPage = BotSettings.countElementsInPage, startPage = State[id].NumerPage }).ToList();
                ChangePage(client, id, mesId, () => GetButtonsFromPage(State[id].NumerPage, buttons, e => e.Fio, e => e.Fio.ToString()));
            }
            else if (query.Data == "LastDevicePage")
            {
                --State[id].NumerPage;
                var devicesPage = getDevicesPage.Get(new PageDto() { countElementsInPage = BotSettings.countElementsInPage, startPage = State[id].NumerPage }).ToList();
                ChangePage(client, id, mesId, () => GetButtonsFromPage(State[id].NumerPage, devicesPage.ToList(), e => e.Name, e => e.Name));
            }
            else if (query.Data == "NextDevicePage")
            {
                ++State[id].NumerPage;
                var devicesPage = getDevicesPage.Get(new PageDto() { countElementsInPage = BotSettings.countElementsInPage, startPage = State[id].NumerPage }).ToList();
                ChangePage(client, id, mesId, () => GetButtonsFromPage(State[id].NumerPage, devicesPage.ToList(), e => e.Name, e => e.Name));
            }
            else if (State[id].ChatState == ChatState.SelectionUser && long.TryParse(query.Data, out State[id].result.UserId))
            {
                await client.EditMessageReplyMarkupAsync(id, mesId);
                State[id].ChatState = ChatState.CommentForTest;
                await client.SendTextMessageAsync(id, "введите комментарий к тесту[необязательно]", replyMarkup: NextCom);
            }
            else if (State[id].ChatState == ChatState.SelectionDevice)
            {
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