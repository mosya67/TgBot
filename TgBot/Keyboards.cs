using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    internal static class Keyboards
    {
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
                InlineKeyboardButton.WithCallbackData("да", "да"),
                InlineKeyboardButton.WithCallbackData("нет", "нет"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("не получилось", "не получилось"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("пропуск", "пропуск"),
            },
        });

        public static InlineKeyboardMarkup next = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("далее", "Next"),
            }
        });

        public static InlineKeyboardMarkup NextCom = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("далее", "NextCom"),
            }
        });

        public static InlineKeyboardMarkup gotoskippedtest = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("да", "gotoYes"),
                InlineKeyboardButton.WithCallbackData("нет", "Save"),
            },
        });

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

        public static InlineKeyboardMarkup release = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("далее", "SetRelease"),
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
