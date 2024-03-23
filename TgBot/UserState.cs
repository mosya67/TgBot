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
        Device,
        Release,
        SkippedQuestion,
        SelectionUser,
        AddNewUser,
    }

    public class UserState
    {
        public ushort QuestNumb = 0;
        public ChatState ChatState;
        public ResultTestDto result;
        public DatesForExcelDTO datesDto;
        public AnswerDto answerDto;
        public IList<Question> Questions;
        public ICollection<int> deleteButtons;
        public bool SkippedTestsFlag = false;
        public sbyte NumerUsersPage = 0;
        
        public UserState()
        {
            result = new();
            answerDto = new();
            datesDto = new();
        }
    }
}
