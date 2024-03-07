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
        public List<int> deleteButtons = new List<int>();
        public bool flag;
        
        public UserState()
        {
            result = new();
            answerDto = new();
            datesDto = new();
        }
    }
}
