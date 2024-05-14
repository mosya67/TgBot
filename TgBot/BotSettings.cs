using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgBot
{
    internal class BotSettings
    {
        /// <summary>
        /// widthButtonsOnPage и heightButtonsOnPage используются для определения шир. и выс. кнопок выбора, например выбор пользователя, после чего ниже них могут быть другие кнопки
        /// </summary>
        public const sbyte widthButtonsOnPage = 2;
        public const sbyte heightButtonsOnPage = 2;

        public const sbyte countElementsInPage = widthButtonsOnPage * heightButtonsOnPage;

        public const string token = "6185570726:AAHBPUqL-qMSrmod9YxV6ot3IKrJ3YXzzCc";
        /// <summary>
        /// количество выводимых последних результатов на выводимую проверку в чек-листе
        /// </summary>
        public const int CountLastResultsInCheck = 3;
    }
}
