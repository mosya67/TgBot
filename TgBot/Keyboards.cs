using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    internal static class Keyboards
    {
        public static InlineKeyboardButton lastUsersPage = InlineKeyboardButton.WithCallbackData("назад", "LastUsersPage");
        public static InlineKeyboardButton nextUsersPage = InlineKeyboardButton.WithCallbackData("далее", "NextUsersPage");

        public static InlineKeyboardMarkup start = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Тесты", "Tests"),
                InlineKeyboardButton.WithCallbackData("Отчеты", "Reports"),
            },
        });

        public static InlineKeyboardMarkup answer = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("PASS", "PASS"),
                InlineKeyboardButton.WithCallbackData("BUG", "BUG"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("BLOCKER", "BLOCKER"),
                InlineKeyboardButton.WithCallbackData("NA", "NA"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("LATER", "LATER"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("PAUSETEST", "PAUSETEST"),
            },
        });

        public static InlineKeyboardMarkup next = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("далее", "NextQuestion"),
            }
        });

        public static InlineKeyboardMarkup NextCom = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("далее", "NextCom"),
            }
        });

        //public static InlineKeyboardMarkup gotoskippedtest = new(new[]
        //{
        //    new[]
        //    {
        //        InlineKeyboardButton.WithCallbackData("да", "gotoYes"),
        //    },
        //});

        public static InlineKeyboardMarkup skiprelease = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("далее", "SkipRelease"),
            }
        });

        public static InlineKeyboardMarkup NextAdCom = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("далее", "NextAdCom"),
            }
        });

        public static InlineKeyboardMarkup backToSelectingUser = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("назад", "BackToSelectingUser"),
            }
        });

        public static InlineKeyboardMarkup backToSelectingDevice = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("назад", "BackToSelectingDevice"),
            }
        });

        public static InlineKeyboardMarkup skipfdate = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("пропустить", "SkipFirstDate"),
            }
        });

        public static InlineKeyboardMarkup skipldate = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("пропустить", "SkipLastDate"),
            }
        });

        public static InlineKeyboardMarkup savedate = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Сохранить", "SaveDates"),
            }
        });
    }
}
