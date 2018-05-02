using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FermaMonitoring
{
    abstract class Command
    {
        public string nameCommand;
        public Ferma ferma;

        public Command(string _name, Ferma _ferma)
        {
            nameCommand = _name;
            ferma = _ferma;
        }

        public abstract void Excecute();
    }

    class CommandServer : Command
    {        
        List<String> exec;        

        public CommandServer(string _name, Ferma _ferma, List<String> _exec) : base (_name, _ferma)
        {
            nameCommand = _name;
            ferma = _ferma;
            exec = _exec;        
        }
                

        public override void Excecute()
        {
            if (exec != null)
            {
                int i = 0;
                while (i < exec.Count)
                {
                    ferma.command = "";

                    string fileName = exec[i];
                    string arg = exec[i + 1];

                    if (arg.Contains("filename"))
                    {
                        string date = DateTime.Now.ToString();
                        date = date.Replace(".", "-");
                        date = date.Replace(":", "_");
                        date = date.Replace(" ", "_");
                        arg = arg.Replace("filename", "ubuntu_server_backup_" + date);
                    }

                    Process pVM = new Process();
                    pVM.StartInfo.FileName = fileName;
                    pVM.StartInfo.Arguments = arg;
                    pVM.StartInfo.UseShellExecute = false;
                    pVM.StartInfo.RedirectStandardOutput = true;
                    pVM.StartInfo.RedirectStandardError = true;
                    pVM.Start();

                    string output = pVM.StandardOutput.ReadToEnd();
                    string error = pVM.StandardError.ReadToEnd();


                    pVM.WaitForExit();
                    

                    FermaMessage message = new FermaMessage();
                    message.NameCommand = nameCommand;
                    message.NameFerma = ferma.name;
                    message.Date = DateTime.Now;
                    message.Priority = 3;
                    message.Text = output + error;

                    ferma.listMessage.Add(message);

                    
                    pVM.Close();

                    i = i + 2;

                    
                    

                }
                
            }
        }
    }

    class CommandStatus : Command
    {
        public CommandStatus(string _name, Ferma _ferma) : base(_name, _ferma)
        {
            nameCommand = _name;
            ferma = _ferma;
        }

        public override void Excecute()
        {            
            float fCPUload = ferma.theCPUCounter.NextValue();
            float fMemUse = ferma.theMemCounter.NextValue()/1000;
            string sCPUload = "CPU load : " + fCPUload.ToString("0") + "%";
            string sMemUse = "Free RAM : " + fMemUse.ToString("0.0") + "Gb";

            ferma.fermaCPU.UpDateSens();

            string tempCPU = "";

            foreach (var sensor in ferma.fermaCPU.listSenTemp)
            {
                tempCPU = tempCPU + sensor.Name + " : " + sensor.Value + "C" + "\n";
            }

            FermaMessage message = new FermaMessage();
            message.NameCommand = nameCommand;
            message.NameFerma = ferma.name;
            message.Date = DateTime.Now;
            message.Priority = 3;
            message.Text = sCPUload + "\n" + sMemUse + "\n" + tempCPU;

            ferma.listMessage.Add(message);

            ferma.command = "";
        }
    }
    
}
