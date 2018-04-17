using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FermaTelegram
{
    class ListMessage
    {
        public List<string> ToClient = new List<string>();
        public List<FermaMessage> FromClient = new List<FermaMessage>();
    }
}
