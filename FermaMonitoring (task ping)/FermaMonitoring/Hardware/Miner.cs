using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FermaMonitoring
{
    class Miner
    {
        public string path;
        public string arg;
        public string name;
        public string procname;
        public Process process = null;

        public Miner()
        { 

        }

        public void GetProcess()
        {
            this.process = null;

            if ((path != "") & (arg != "") & (name != "") & (procname != ""))
            { 
                Process[] procs = Process.GetProcesses();
                foreach (Process p in procs)
                {
                    if (p.ProcessName == procname)
                    {
                        this.process = p;
                        Console.WriteLine(p.ProcessName);
                        break;
                    }
                }
            }
        }
    }
}
