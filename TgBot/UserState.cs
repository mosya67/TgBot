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
        SetNewFio,
        CommentForTest,
        AdditionalCommentForTest,
        FirtsDate,
        LastDate,
        Device,
        Release,
    }

    public class UserState
    {
        public int QuestNumb;
        public ChatState ChatState;
        public ResultTestDto result;
        public DatesForExcelDTO datesDto;
        public AnswerDto answerDto;
        public IList<Question> Questions;
        public bool flag;
        public bool flag2;

        public UserState()
        {
            result = new();
            answerDto = new();
            datesDto = new();
        }
    }
}
