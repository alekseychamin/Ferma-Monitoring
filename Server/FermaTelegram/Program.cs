using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FermaTelegram
{
    class Program
    {
        public static Application app;

        static void Main(string[] args)
        {
            app = new Application("setting_telegram.xml", "log.txt");            
    
            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}
