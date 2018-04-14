using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Management;

namespace FermaMonitoring
{
    class Program
    {
        

        static void Main(string[] args)
        {
            Ferma ferma = new Ferma(Environment.MachineName, "setting_ferma.xml");

            ferma.InitVideoCard();
            ferma.getHardwareInformation.Start();            
            ferma.executeCommand.Start();
            ferma.send.Start();
            ferma.StartCurrentMiner(firstStart: true);                        

            while (true)
            {
                Thread.Sleep(100);
            }            
           
        }       
    }
}
