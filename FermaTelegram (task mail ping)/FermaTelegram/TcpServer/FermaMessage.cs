using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FermaTelegram
{
    class FermaMessage
    {
        public string NameFerma { get; set; }
        public DateTime Date { get; set; }
        public string NameCommand { get; set; }
        public string Text { get; set; }
        public int Priority { get; set; }
        // 1 предупреждения о наличии событий (температура и т.д.)
        // 2 собщения без входящей команды
        // 3 сообщения о выполнении команды

        public string Type { get; set; }
    }
}
