using Domain.Dto;
using Domain.Model;
using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace TelegramBot
{
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
    }

    public class UserState
    {
        public sbyte QuestNumb = 0;
        public ChatState ChatState;
        public ResultTestDto result;
        public DatesForExcelDTO datesDto;
        public IList<Question> Questions;
        public ICollection<int> deleteButtons;
        public bool SkippedTestsFlag = false;
        public sbyte NumerPage = 0;
        public bool PassingStoppedTest = false;
        public int ResultId;

        public UserState()
        {
            result = new();
            datesDto = new();
        }
    }
}
