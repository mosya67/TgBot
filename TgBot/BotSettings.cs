using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgBot
{
    internal static class BotSettings
    {
        /// <summary>
        /// widthButtonsOnMessage и heightButtonsOnMessage используются для определения шир. и выс. кнопок выбора, например выбор пользователя, после чего ниже них могут быть другие кнопки
        /// </summary>
        public const sbyte widthButtonsOnMessage = 2;
        public const sbyte heightButtonsOnMessage = 2;

        public const sbyte countElementsInPage = widthButtonsOnMessage * heightButtonsOnMessage;

        public const string token = "6185570726:AAHBPUqL-qMSrmod9YxV6ot3IKrJ3YXzzCc";
    }
}
