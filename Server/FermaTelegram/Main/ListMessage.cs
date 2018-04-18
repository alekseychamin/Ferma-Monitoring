using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FermaTelegram
{
    class ListMessage
    {
        public List<string> commandFerma = new List<string>(); // команда в писмьме для 
        public List<string> commandServer = new List<string>();
        public List<FermaMessage> reply = new List<FermaMessage>();
    }
}
