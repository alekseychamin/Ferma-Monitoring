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

        public List<VideoCard> listVideoCard;
        public List<Command> listCommand;
        public List<FermaMessage> listMessage;
        public List<Miner> listMiner;
        
        public TcpClient tcpClient;
        public ModbusTCP modbusTCP;

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

            modbusTCP = new ModbusTCP(this, this.IPaddrClient);

            CommandTemp comTemp = new CommandTemp("/temp", this);
            listCommand.Add(comTemp);

            CommandMaxTemp comMaxTemp = new CommandMaxTemp("/maxtemp", this);
            listCommand.Add(comMaxTemp);

            CommandCont comCont = new CommandCont("/vent", this);
            listCommand.Add(comCont);

            CommandStop comStop = new CommandStop("/stop", this);
            listCommand.Add(comStop);

            CommandMiner comZECMiner = new CommandMiner("/ZEC", this, "ZEC");
            listCommand.Add(comZECMiner);

            CommandMiner comZCLMiner = new CommandMiner("/ZCL", this, "ZCL");
            listCommand.Add(comZCLMiner);

            CommandMiner comVTCMiner = new CommandMiner("/VTC", this, "VTC");
            listCommand.Add(comVTCMiner);

            CommandMiner comBTGMiner = new CommandMiner("/BTG", this, "BTG");
            listCommand.Add(comBTGMiner);

            CommandMiner comMusicMiner = new CommandMiner("/Music", this, "Music");
            listCommand.Add(comMusicMiner);

            CommandMiner comETHMiner = new CommandMiner("/ETH", this, "ETH");
            listCommand.Add(comETHMiner);

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

                if (xnode.Name == "miner")
                {
                    if (xnode.Attributes.Count > 0)
                    {
                        XmlNode attr = xnode.Attributes.GetNamedItem("miner");
                        Miner miner = new Miner();

                        foreach (XmlNode childnode in xnode.ChildNodes)
                        {                           
                            if (childnode.Name == "path")
                            {
                                miner.path = childnode.InnerText;
                            }
                            if (childnode.Name == "arg")
                            {
                                miner.arg = childnode.InnerText;
                            }
                            if (childnode.Name == "name")
                            {
                                miner.name = childnode.InnerText;
                            }
                            if (childnode.Name == "procname")
                            {
                                miner.procname = childnode.InnerText;
                            }
                        }
                        listMiner.Add(miner);
                    }
                }

                if (xnode.Name == "current_miner")
                {
                    if (xnode.Attributes.Count > 0)
                    {
                        XmlNode attr = xnode.Attributes.GetNamedItem("current_miner");
                        Miner miner = new Miner();

                        foreach (XmlNode childnode in xnode.ChildNodes)
                        {                            
                            if (childnode.Name == "name")
                            {
                                this.currentMiner.name = childnode.InnerText;
                            }
                        }
                        listMiner.Add(miner);
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
                modbusTCP.SendData();
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
