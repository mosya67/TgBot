using Telegram.Bot;
using Telegram.Bot.Types;
using static TelegramBot.Keyboards;
using Telegram.Bot.Types.Enums;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Diagnostics;
using ExcelServices;
using System.Net.Http;
using System.Net;
using System.IO;
using Domain;
using Domain.Dto;
using Domain.Model;
using System.ComponentModel.DataAnnotations;
using Database.GetFunctions;
using Database.AddFunctions;

namespace TelegramBot
{
    internal partial class Program
    {
        const string token = "6185570726:AAHBPUqL-qMSrmod9YxV6ot3IKrJ3YXzzCc";
        static string[] _commands = {"/start", "/resetname"};
        static Dictionary<long, UserState> State;

        static IExcelGenerator<Task<FileDto>, DatesForExcelDTO> excel;
        static IGetCommand<Test, ushort> getTest;
        static IGetCommand<string, long> getUserName;
        static IGetCommand<IList<Test>> getSortedTests;
        static IWriteCommand<IReadOnlyList<ValidationResult>, ResultTestDto> saveResult;

        static void Main(string[] args)
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
            //else if (message?.Text?.ToLower() == "/resetname")
            //{
            //    await client.SendTextMessageAsync(id, "Введите новое фио");
            //    State[id].ChatState = ChatState.SetNewFio;
            //    return;
            //}
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

                State[id].result.UserName = getUserName.Get(id);
                if (State[id].result.UserName == null)
                {
                    State[id].result.UserId = id;
                    await client.SendTextMessageAsync(id, "Введите фио");
                    State[id].ChatState = ChatState.SetFio;
                }
                else
                {
                    State[id].result.UserId = id;
                    State[id].ChatState = ChatState.CommentForTest;
                    await client.SendTextMessageAsync(id, "введите комментарий к тесту[необязательно]", replyMarkup: NextCom);
                }
            }
            else if (State[id].ChatState == ChatState.SetFio)
            {
                State[id].result.UserName = message.Text;
                await client.SendTextMessageAsync(id, "введите комментарий к тесту[необязательно]", replyMarkup: NextCom);
                State[id].ChatState = ChatState.CommentForTest;
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
            //else if (State[id].ChatState == ChatState.SetNewFio)
            //{
            //    State[id].result.UserName = message.Text;
            //    await client.SendTextMessageAsync(id, "имя изменено");
            //    State[id].ChatState = ChatState.None;
            //}
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
            else if (State[id].ChatState == ChatState.Device)
            {
                State[id].result.Device = message.Text;
            }
            else if (State[id].ChatState == ChatState.Release)
            {
                State[id].result.Release = message.Text;
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
                State[id].ChatState = ChatState.Device;
                await client.EditMessageReplyMarkupAsync(id, mesId);
                await client.SendTextMessageAsync(id, "аппарат:", replyMarkup: release);
            }
#warning че это за хуйня
            else if (query.Data == "SetRelease")
            {
                State[id].ChatState = ChatState.Release;
                await client.EditMessageReplyMarkupAsync(id, mesId);
                await client.SendTextMessageAsync(id, "релиз:", replyMarkup: skiprelease);
            }
#warning че это за хуйня
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
            else if (query.Data == "NextQuestion")
            {
                if (!State[id].SkippedTestsFlag)
                {
                    State[id].QuestNumb++;
                    if (State[id].QuestNumb < State[id].Questions.Count())
                    {
                        State[id].ChatState = ChatState.AnswersTheQuestion; // <--- просто продолжение прохождение вопросов (не пропущенных)
                        NextQuestion(client, id, State[id].QuestNumb);
                        return;
                    }
                }

                var question = GetNumberSkippedQuestion(client, id);

                if (question.HasValue)
                {
                    await client.SendTextMessageAsync(id, "Ваш пропуск:");
                    State[id].QuestNumb = question.Value;
                    State[id].SkippedTestsFlag = true;
                    State[id].ChatState = ChatState.AnswersTheQuestion;
                    NextQuestion(client, id, State[id].QuestNumb);
                    return;
                }

                SaveTestResult(client, id);
            }
        }
    }
}
