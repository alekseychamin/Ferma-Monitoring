using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FermaMonitoring
{
    class TcpClient
    {
        private static string connectMessage = "online";
        private bool sendBusy;
        public int port;
        public string IPaddrServer;

        private Task TcpWork;

        private Task sendConnect;
        private Task checkConnect;

        private Ferma ferma;
        private IPEndPoint ipPoint;
        public bool isConnect = false;
        private string serverMessage = "";
        private Socket socket;

        public TcpClient(Ferma _ferma)
        {            
            ferma = _ferma;
            port = ferma.port;
            IPaddrServer = ferma.IPaddrServer;
            
            TcpWork = new Task(ReciveCommand);
            TcpWork.Start();

            //sendConnect = new Task(SendConnect);
            //checkConnect = new Task(CheckConnect);

            //sendConnect.Start();
            //checkConnect.Start();

        }

        public void Connect()
        {
            try
            {
                ipPoint = new IPEndPoint(IPAddress.Parse(IPaddrServer), port);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);                

                socket.Connect(ipPoint);

                FermaMessage message = new FermaMessage();                
                message.NameFerma = ferma.name;
                message.Date = DateTime.Now;
                message.Priority = 2;
                message.Text = "app FermaMonitoring запущена!";
                //message.Type = "system";
                                
                ferma.listMessage.Add(message);

                isConnect = true;                                

                Console.WriteLine(message);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString() + " :" + ferma.name + " Error in Connect SocketTcpClient : " + ex.Message);                
                isConnect = false;                
            }
        }
      
        public void ReciveCommand()
        {            
            if (isConnect)
            {
                try
                {
                    byte[] data = new byte[4096]; // буфер для ответа
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // количество полученных байт

                    do
                    {                                                        
                            
                        bytes = socket.Receive(data, data.Length, 0);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        Console.WriteLine(DateTime.Now.ToString() + " Ferma.TcpClient.ReciveCommand - try to recive command" + data);                            

                    } while (socket.Available > 0);

                    string json = builder.ToString();                        

                    if (json != "")
                    {

                        //Console.WriteLine(DateTime.Now.ToString() + " :" + ferma.name + " Ferma.TcpClient.ReciveCommand recived command from server : " + builder.ToString());
                           
                        isConnect = true;

                        FermaMessage message = JsonConvert.DeserializeObject<FermaMessage>(json);

                        if (message.Type == "command")
                            ferma.command = message.Text;                            

                    }
                    else
                    {
                        isConnect = false;
                    }
                }
                catch (Exception ex)
                {

                    isConnect = false;
                    Console.WriteLine(DateTime.Now.ToString() + " :" + ferma.name + " Error in ReciveCommand SocketTcpClient : " + ex.Message);
                    //ferma.listMessage.Add("Сервер был перезапушен!");
                        
                }
            }
            else
            {
                Connect();
            }                        
        }
               
        public int SendData(FermaMessage message)
        {
            int result = 0;

            if ((isConnect) && (message != null) && (!sendBusy))
            {
                sendBusy = true;
                string serialized = JsonConvert.SerializeObject(message);
                try
                {
                    byte[] data = Encoding.Unicode.GetBytes(serialized);
                    result = socket.Send(data);
                    //Console.WriteLine(DateTime.Now.ToString() + " Ferma.TcpClient.SendData " + message);
                    Thread.Sleep(50);                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now.ToString() + " :" + ferma.name + " Error in SendData : " + ex.Message);
                    isConnect = false;
                }
                sendBusy = false;                
            }
            return result;
        }        

    }
}
