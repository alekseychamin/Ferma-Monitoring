using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;
using System.Xml;
using System.Diagnostics;

namespace FermaMonitoring
{
    class Ferma
    {

        private static Computer computer
         = new Computer()
         {
             GPUEnabled = true,
             CPUEnabled = true
         };

        public CPUclass fermaCPU;

        public bool firststartMiner;
        public string filenameSetting;
        public Miner currentMiner;
        public string name;
        public string command;
        public string IPaddrServer;
        public string IPaddrClient;
        public int port;
        public int delayHardwareTime;
        public int delayShowMessage;
        public int delayStartMiner;
        public PerformanceCounter theCPUCounter;
        public PerformanceCounter theMemCounter;

        public List<VideoCard> listVideoCard;
        public List<Command> listCommand;
        public List<FermaMessage> listMessage;
        public List<Miner> listMiner;
        
        public TcpClient tcpClient;
        

        public Task getHardwareInformation;
        public Task executeCommand;
        public Task send;
        

        public Ferma(string _name, string filename)
        {
            name = _name;
            computer.Open();            
            filenameSetting = filename;

            currentMiner = new Miner();

            listVideoCard = new List<VideoCard>();
            listCommand = new List<Command>();
            listMessage = new List<FermaMessage>();
            listMiner = new List<Miner>();

            LoadXML(filename);

            theCPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            theMemCounter = new PerformanceCounter("Memory", "Available MBytes");
            //-----------------------------------------------------------------------

            List<string> startServerCommand = new List<string>();
            startServerCommand.Add("VBoxManage");
            startServerCommand.Add("startvm \"Ubuntu Server\" --type headless");

            CommandServer startServer = new CommandServer("startserver", this, startServerCommand);
            listCommand.Add(startServer);

            //-----------------------------------------------------------------------

            List<string> stopServerCommand = new List<string>();
            stopServerCommand.Add("VBoxManage");
            stopServerCommand.Add("controlvm \"Ubuntu Server\" poweroff --type headless");

            CommandServer stopServer = new CommandServer("stopserver", this, stopServerCommand);
            listCommand.Add(stopServer);

            //-----------------------------------------------------------------------

            List<string> shutdownServerCommand = new List<string>();
            shutdownServerCommand.Add("VBoxManage");
            shutdownServerCommand.Add("controlvm \"Ubuntu Server\" poweroff --type headless");

            shutdownServerCommand.Add(@"c:\windows\System32\shutdown.exe");
            shutdownServerCommand.Add("/s");

            CommandServer shutdownServer = new CommandServer("shutdown", this, shutdownServerCommand);
            listCommand.Add(shutdownServer);

            //-----------------------------------------------------------------------

            List<string> backupServerCommand = new List<string>();
            backupServerCommand.Add("VBoxManage");
            backupServerCommand.Add("controlvm \"Ubuntu Server\" poweroff --type headless");

            backupServerCommand.Add("VBoxManage");
            backupServerCommand.Add("export \"Ubuntu Server\" --output " + @"D:\VM\Backup\filename.ova");

            CommandServer backupServer = new CommandServer("backup", this, backupServerCommand);
            listCommand.Add(backupServer);
            
            //-----------------------------------------------------------------------

            List<string> backupShutdownServerCommand = new List<string>();
            backupShutdownServerCommand.Add("VBoxManage");
            backupShutdownServerCommand.Add("controlvm \"Ubuntu Server\" poweroff --type headless");

            backupShutdownServerCommand.Add("VBoxManage");
            backupShutdownServerCommand.Add("export \"Ubuntu Server\" --output " + @"D:\VM\Backup\filename.ova");

            backupShutdownServerCommand.Add(@"c:\windows\System32\shutdown.exe");
            backupShutdownServerCommand.Add("/s");

            CommandServer backupHShutdownServer = new CommandServer("backup_shutdown", this, backupShutdownServerCommand);
            listCommand.Add(backupHShutdownServer);

            //-----------------------------------------------------------------------

            CommandStatus statusServer = new CommandStatus("status", this);
            listCommand.Add(statusServer);

            tcpClient = new TcpClient(this);

            getHardwareInformation = new Task(GetHardwareInformation);
            executeCommand = new Task(ExecuteCommand);
            send = new Task(SendMessage);                       
                        
        }

        public void ChangeCurMiner()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filenameSetting);
            XmlElement xRoot = xmlDoc.DocumentElement;
            
            foreach (XmlNode xnode in xRoot)
            {
                if (xnode.Name == "current_miner")
                {
                    xRoot.RemoveChild(xnode);
                    break;
                }
            }

            XmlElement current_minerElem = xmlDoc.CreateElement("current_miner");
            XmlAttribute current_minerAttr = xmlDoc.CreateAttribute("current_miner");
            XmlText current_minerAttrName = xmlDoc.CreateTextNode(currentMiner.name);
            XmlElement current_minerName = xmlDoc.CreateElement("name");
            
            XmlText current_miner = xmlDoc.CreateTextNode(currentMiner.name);
            
            current_minerAttr.AppendChild(current_minerAttrName);
            current_minerName.AppendChild(current_miner);
            current_minerElem.Attributes.Append(current_minerAttr);
            current_minerElem.AppendChild(current_minerName);
            xRoot.AppendChild(current_minerElem);
            xmlDoc.Save(filenameSetting);
        }

        public void LoadXML(string filename)
        {
            string path = Directory.GetCurrentDirectory();
            filenameSetting = path + "\\" + filename;
            Console.WriteLine("Current directory for xml: " + filenameSetting);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filenameSetting);
            XmlElement xRoot = xmlDoc.DocumentElement;
            
            foreach (XmlNode xnode in xRoot)
            {
                if (xnode.Name == "time")
                {
                    if (xnode.Attributes.Count > 0)
                    {
                        XmlNode attr = xnode.Attributes.GetNamedItem("time");
                        foreach (XmlNode childnode in xnode.ChildNodes)
                        {
                            if (childnode.Name == "delayHardwareTime_sec")
                            {
                                delayHardwareTime = Convert.ToInt32(childnode.InnerText) * 1000;
                            }
                            if (childnode.Name == "delayShowMessage_min")
                            {
                                delayShowMessage = Convert.ToInt32(childnode.InnerText) * 60 * 1000;
                            }
                            if (childnode.Name == "delayStartMiner_min")
                            {
                                delayStartMiner = Convert.ToInt32(childnode.InnerText) * 60 * 1000;
                            }                            
                        }
                    } 
                }

                if (xnode.Name == "network")
                {
                    if (xnode.Attributes.Count > 0)
                    {
                        XmlNode attr = xnode.Attributes.GetNamedItem("network");
                        foreach (XmlNode childnode in xnode.ChildNodes)
                        {
                            if (childnode.Name == "IPaddressClient")
                            {
                                IPaddrClient = childnode.InnerText;
                            }
                            if (childnode.Name == "IPaddressServer")
                            {
                                IPaddrServer = childnode.InnerText;
                            }
                            if (childnode.Name == "port")
                            {
                                port = Convert.ToInt32(childnode.InnerText);
                            }
                        }
                    }
                }                
            }

        }

        public void UpdateProcessMiner()
        {
            foreach (Miner miner in listMiner)
            {
                miner.GetProcess();

                if (miner.name == currentMiner.name)
                {
                    currentMiner.name = miner.name;
                    currentMiner.path = miner.path;
                    currentMiner.arg = miner.arg;
                    currentMiner.process = miner.process;
                }
            }
        }

        public void StartCurrentMiner(bool firstStart)
        {
            bool curMinerStart = false;
            bool switchMiner = false;                        

            UpdateProcessMiner();

            firststartMiner = firstStart;

            if (currentMiner.process != null)
                curMinerStart = true;

            if (!curMinerStart)
            {
                FermaMessage message;

                foreach (Miner miner in listMiner)
                {
                    if ((miner.process != null) & (miner.name != currentMiner.name))
                    {
                        switchMiner = true;
                        miner.process.Kill();

                        message = new FermaMessage();
                        //message.NameCommand = name;
                        message.NameFerma = name;
                        message.Date = DateTime.Now;
                        message.Priority = 2;
                        message.Text = miner.name + " майнер остановлен!";
                        
                        //Console.WriteLine(message);

                        listMessage.Add(message);
                    }
                }

                if (firstStart)
                {
                    message = new FermaMessage();
                    //message.NameCommand = name;
                    message.NameFerma = name;
                    message.Date = DateTime.Now;
                    message.Priority = 2;
                    message.Text = currentMiner.name + " запуск через " + (delayStartMiner / 60000) + " м";
                    
                    listMessage.Add(message);
                    Thread.Sleep(delayStartMiner); // задержка времени перед первым запуском майнера
                    firststartMiner = false;
                }
                else
                    if (switchMiner)
                    Thread.Sleep(5000);


                Process.Start(currentMiner.path, currentMiner.arg);

                message = new FermaMessage();
                //message.NameCommand = name;
                message.NameFerma = name;
                message.Date = DateTime.Now;
                message.Priority = 2;
                message.Text = currentMiner.name + " майнер запущен!";

                //Console.WriteLine(message);

                listMessage.Add(message);
            }
            else
                firststartMiner = false;
        }

        public void StopMiner()
        {
            foreach (Miner miner in listMiner)
            {
                miner.GetProcess();
                if (miner.process != null)
                {
                    miner.process.Kill();
                }
            }
        }

        private string GetNumVideoCard(string indentifier)
        {
            string res = "";
            bool b = indentifier.Contains("/");
            int index1 = indentifier.IndexOf("/");
            int index2 = indentifier.IndexOf("/", index1 + 1);

            res = indentifier.Substring(index2 + 1, indentifier.Length - index2 - 1);

            return res;
        }

        private string GetNameVideoCard(string name)
        {
            string res = "";
            string n = "NVIDIA GeForce";
            bool b = name.Contains(n);
            if (b)
            {
                int index = name.IndexOf(n);
                res = name.Substring(index + n.Length, name.Length - n.Length);
            }

            return res;
        }

        public void InitCPU()
        {
            foreach (var hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.CPU)
                {
                    CPUclass cpu = new CPUclass();
                    cpu.hardware = hardware;

                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            //Console.WriteLine("temp CPU " + sensor.Name + " " + sensor.Hardware + " " + sensor.Value.GetValueOrDefault());
                            cpu.listSenTemp.Add(sensor);
                        }
                    }

                    this.fermaCPU = cpu;
                }
            }
        }

        public void InitVideoCard()
        {
            listVideoCard.Clear();

            foreach (var hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.GpuNvidia)
                {                    
                    VideoCard videocard = new VideoCard();

                    videocard.name = "№" + GetNumVideoCard(hardware.Identifier.ToString()) + " " + GetNameVideoCard(hardware.Name);
                    videocard.identifier = hardware.Identifier.ToString();
                    videocard.hardware = hardware;
                    videocard.delayShowMessage = delayShowMessage;
                    listVideoCard.Add(videocard);

                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            videocard.senTemp = sensor;
                        }

                        if (sensor.SensorType == SensorType.Control)
                        {
                            videocard.senCont = sensor;
                        }
                    }
                    videocard.UpDateSens();

                    videocard.checkMaxTemp.Start();
                    videocard.checkMinTemp.Start();

                    videocard.checkMaxCont.Start();
                    videocard.checkMinCont.Start();
                }
            }
        }    

        public void GetHardwareInformation()
        {
            while (true)
            {
                UpDateGPUInformation();                
                //DisplayGPUInformation();
                Thread.Sleep(delayHardwareTime);                
            }
        }

        public void UpDateGPUInformation()
        {
            string mesVideoCard = "";
            foreach (var videocard in listVideoCard.ToArray())
            {
                videocard.UpDateSens();
                try
                {
                    foreach (var mes in videocard.listMessage)
                    {
                        if (mes != "")
                        {
                            mesVideoCard = mesVideoCard + mes + "\n";
                            videocard.listMessage.Remove(mes);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now.ToString() + " error in UpDateGPUInformation count of element >");
                    break;
                }
            }
            
            if (mesVideoCard != "")
            {

                FermaMessage message = new FermaMessage();
                //message.NameCommand = name;
                message.NameFerma = name;
                message.Date = DateTime.Now;
                message.Priority = 1;

                message.Text = mesVideoCard;

                listMessage.Add(message);
            }
        }

        public string GetTimeWorkMiner()
        {
            string res = "";

            if (this.currentMiner.process != null)
            {
                DateTime startMiner = this.currentMiner.process.StartTime;
                DateTime currentDate = DateTime.Now;
                
                //int diff_h = (currentDate - startMiner).Hours;
                TimeSpan diff = currentDate.Subtract(startMiner);

                res = this.currentMiner.name + " майнер запущен: " + diff.ToString(@"dd\.hh\:mm\:ss") + " дней";                
            }
            else
                res = this.currentMiner.name + " майнер не запущен:" + " -- ";


            return res;
        }

        public String GetGPUFermaTemp()
        {
            String res = "";
            foreach (var videocard in listVideoCard.ToArray())
            {
                res = res + videocard.name + ": " + "t = " + "*" + videocard.temperature + "*" + " C" + "\n";
            }

            res = GetTimeWorkMiner() + "\n" + res;                

            return res;
        }

        public String GetGPUFermaMaxTemp()
        {
            String res = "";
            float maxTemp = 0;

            foreach (var videocard in listVideoCard.ToArray())
            {
                if (videocard.temperature > maxTemp)
                {
                    maxTemp = videocard.temperature;
                    res = videocard.name + ": " + "t = " + "*" + videocard.temperature + "*" + " C";
                }
            }             

            res = GetTimeWorkMiner() + "\n" + res;

            return res;
        }

        public String GetGPUFermaCont()
        {
            String res = "";
            foreach (var videocard in listVideoCard.ToArray())
            {
                res = res + videocard.name + ": " + "vent = " + "*" + videocard.control + "*" + " %" + "\n";
            }

            res = GetTimeWorkMiner() + "\n" + res;

            return res;
        }

        public void DisplayGPUInformation()
        {
            foreach (var videocard in listVideoCard.ToArray())
            {
                Console.WriteLine(DateTime.Now.ToString() + ": " + name + " :" + videocard.name + ": " + "temp = " +
                                    videocard.temperature + "C : cont = " + videocard.control + "%");
            }
        }

        public void SendMessage()
        {
            while (true)
            {
                int i = 0;                
                
                if (tcpClient.isConnect)
                {
                    try
                    {
                        foreach (FermaMessage message in listMessage.ToArray())
                        {
                            
                            //Console.WriteLine(DateTime.Now.ToString() + " отправка сообщении серверу Ferma.SendMessage " + message);                                
                            if (tcpClient.SendData(message) > 0)
                                listMessage.Remove(message);                                
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(DateTime.Now.ToString() + "error in Ferma.SendMessage count message " + ex.Message);                        
                    }
                }                                
                Thread.Sleep(100);                              
            }
        }

        public void ExecuteCommand()
        {
            while (true)
            {
                foreach (var command in listCommand)
                {
                    if (command.nameCommand == this.command)
                    {
                        command.Excecute();
                    }
                }
                Thread.Sleep(100);
            }
        }

    }
}
