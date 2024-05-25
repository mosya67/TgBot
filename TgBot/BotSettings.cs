using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace TgBot
{
    internal class BotSettings
    {
        /// <summary>
        /// widthButtonsOnPage и heightButtonsOnPage используются для определения шир. и выс. кнопок выбора, например выбор пользователя, после чего ниже них могут быть другие кнопки
        /// </summary>
        public sbyte widthButtonsOnPage;
        public sbyte heightButtonsOnPage;
        [JsonIgnore]
        public sbyte countElementsInPage
        {
            get
            {
                return (sbyte)(widthButtonsOnPage * heightButtonsOnPage);
            }
        }
        [JsonIgnore]
        public string token = "6185570726:AAHBPUqL-qMSrmod9YxV6ot3IKrJ3YXzzCc";
        /// <summary>
        /// количество выводимых последних результатов на выводимую проверку в чек-листе
        /// </summary>
        public sbyte CountLastResultsInCheck;
    }
}
