using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Threading;

namespace FermaTelegram
{
    class Application
    {
        public string filenameSetting;
        public string filenameLog;
        public int port;
        public string IPaddress;
        public int delayShowMessage;        
        public List<Command> listCommand;
        public ListMessage listMessage;

        public TcpServer tcpServer;        
        public MakeParse makeParse;
        public MailCommand mailCommand;        

        public Task executeCommand;        


        private void SaveLogMessage(string message)
        {
            message = DateTime.Now.ToString() + " : " + message + "\n";

            var txt = new StringBuilder();
            txt.Append(message);

            if (File.Exists(filenameLog))
            {
                try
                {
                    File.AppendAllText(filenameLog, txt.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                File.WriteAllText(filenameLog, txt.ToString());
            }

        }

        public Application(string filenameSetting, string filenameLog)
        {
            listMessage = new ListMessage();

            this.filenameSetting = filenameSetting;
            this.filenameLog = Directory.GetCurrentDirectory() + "\\" + filenameLog;

            LoadXML(filenameSetting);

            listCommand = new List<Command>();

            tcpServer = new TcpServer(port, IPaddress, listMessage);

            tcpServer.RegisterHandler(SaveLogMessage);
            

            mailCommand = new MailCommand("pop.mail.ru", 995, true, "ale-san2006@mail.ru", "260686zvezda");

            mailCommand.RegisterHandler(SaveLogMessage);            

            makeParse = new MakeParse(listMessage);            
            
            tcpServer.delayShowMessage = delayShowMessage;

            CommandStatusZec commandStatusZec = new CommandStatusZec("/status", makeParse);
            listCommand.Add(commandStatusZec);

            CommandStatusEth commandStatusEth = new CommandStatusEth("/statusEth", makeParse);
            listCommand.Add(commandStatusEth);

            CommandStatusZcl commandStatusZcl = new CommandStatusZcl("/statusZcl", makeParse);
            listCommand.Add(commandStatusZcl);

            executeCommand = new Task(ExecuteCommand);
            executeCommand.Start();

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
                            if (childnode.Name == "delayShowMessage_min")
                            {
                                delayShowMessage = Convert.ToInt32(childnode.InnerText) * 60 * 1000;
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
                            if (childnode.Name == "port")
                            {
                                port = Convert.ToInt32(childnode.InnerText);
                            }
                            if (childnode.Name == "IPaddress")
                            {
                                IPaddress = childnode.InnerText;
                            }
                        }
                    }
                }

                if (xnode.Name == "bot")
                {
                    if (xnode.Attributes.Count > 0)
                    {
                        XmlNode attr = xnode.Attributes.GetNamedItem("bot");
                        
                        foreach (XmlNode childnode in xnode.ChildNodes)
                        {
                            if (childnode.Name == "chatID")
                            {
                                //ChatId = Convert.ToInt32(childnode.InnerText);
                                //ChatIdWarning = Convert.ToInt32(childnode.InnerText);
                            }
                            if (childnode.Name == "token")
                            {
                                //token = childnode.InnerText;
                            }                            
                        }                        
                    }
                }               
            }

        }

        public void ExecuteCommand()
        {
            while (true)
            {

                int i = 0;
                string message;

                while (i < listMessage.command.Count)
                {
                    message = listMessage.command[i];

                    foreach (var command in listCommand)
                    {
                        if (command.name == message)
                        {
                            command.Excecute();
                        }
                    }
                    listMessage.command.Remove(message);
                }
                
                Thread.Sleep(100);
            }
        }
    }
}
