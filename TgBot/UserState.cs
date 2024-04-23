using Domain.Dto;
using Domain.Model;
using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace TelegramBot
{
    public class UserState
    {
        public sbyte QuestNumb = 0;
        public ChatState ChatState;
        public ResultTestDto result;
        public ReportExcelDTO datesDto;
        public IList<Question> Questions;
        public int[] deleteButtons;
        public bool SkippedTestsFlag = false;
        public sbyte NumerPage = 0;
        public bool PassingStoppedTest = false;
        public int ResultId;
        public IList<Answer> LastAnswers;
        public UserRole Role = UserRole.None;

        public UserState()
        {
            result = new();
            datesDto = new();
        }
    }
    public enum ChatState
    {
        None,
        SelectingTest,
        AnswersTheQuestion,
        Commenting,
        SetFio,
        CommentForTest,
        AdditionalCommentForTest,
        FirtsDate,
        LastDate,
        Release,
        SkippedQuestion,
        SelectionUser,
        AddNewUser,
        AddNewDevice,
        SelectionDevice,
        SelectingStoppedTest,
        ChangeTest,
        AddTest,
        SelectingTestToReports,
        GetReport,
        WriteLastResult,
    }

    public enum UserRole
    {
        None,
        Admin,
    }
}
