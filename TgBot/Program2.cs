using StatusGeneric;
using System;
using System.Runtime.CompilerServices;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot
{
    internal partial class Program
    {
        public static IStatusGeneric<DateTime> ParseDate(string message)
        {
            DateTime date;
            var status = new StatusGenericHandler<DateTime>();
            if (DateTime.TryParse(message, out date))
            {
                if (date == default)
                {
                    status.AddError("некорректная дата");
                    return status;
                }

                status.SetResult(date);
            }
            return status;
        }
    }
}
