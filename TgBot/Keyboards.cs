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
        public static InlineKeyboardMarkup selectVariantReport = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Последнее тестирование данного комплекса", "1"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Последние 3 тестирования данного комплекса", "2"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Все тестирования данного комплекса", "3")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Все тестирования данного комплекса за указанные даты", "4"),
            },
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

        public static InlineKeyboardMarkup changeQuestionsAndLogin = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("редактировать", "ChangeQuestions"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("войти", "Login"),
            }
        });

        public static InlineKeyboardMarkup skipfdate = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("пропустить", "SkipFirstDate"),
            }
        });

        public static InlineKeyboardMarkup roles = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("обычный пользователь", "RoleNone"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("админ", "RoleAdmin"),
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
