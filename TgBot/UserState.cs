using Domain.Dto;
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
        public int QuestNumb { get; set; }
        public ChatState ChatState { get; set; }
        public AnswerDto dto { get; set; }
        public List<Question> Questions { get; set; }
        public bool flag { get; set; }
        public bool flag2 { get; set; }

        public UserState()
        {
            dto = new();
            Questions = new();
        }
    }
}
