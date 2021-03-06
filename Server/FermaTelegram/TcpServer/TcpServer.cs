﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FermaTelegram
{
   
    class TcpServer
    {
        private int port;

        public int delayShowMessage;

        public List<string> listMessageFromTelegram;
        public List<FermaMessage> listMessageToTelegram;

        public Socket listenSocket;
        public IPEndPoint ipPoint;
        public List<ClientObject> listClient;
       

        public delegate void SendErrorLog(string message);
        SendErrorLog _del;

        public void RegisterHandler(SendErrorLog del)
        {
            _del = del;
        }

        public TcpServer(int _port, string IPaddress)
        {

            port = _port;
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress localAddr = IPAddress.Parse(IPaddress);
            // получаем адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(localAddr, port);

            // создаем сокет
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // связываем сокет с локальной точкой, по которой будем принимать данные
            listenSocket.Bind(ipPoint);

            // начинаем прослушивание
            listenSocket.Listen(10);

            Console.WriteLine("Сервер запущен. Ожидание подключений по адресу : " + IPaddress + ":" + port);

            listClient = new List<ClientObject>();            

            listMessageFromTelegram = new List<string>();
            listMessageToTelegram = new List<FermaMessage>();


            Task getNewClient = new Task(GetNewClient);
            Task tcpSend = new Task(SendCommand);
            Task tcpRecive = new Task(ReciveMessageFromClient);
            Task checkClient = new Task(CheckClient);

            getNewClient.Start();
            tcpSend.Start();
            tcpRecive.Start();
            checkClient.Start();

        }

        public void CheckClient()
        {
            Ping ping = new Ping();
            

            while (true)
            {

                PingReply pingReply = null;

                try
                {
                    int i = 0;
                    while (i < listClient.Count)
                    {
                        ClientObject client = listClient[i];
                        pingReply = ping.Send(client.IPAddr);

                        if ((pingReply.Status != IPStatus.Success) || (!client.connect))
                        {
                            FermaMessage message = new FermaMessage();
                            message.Date = DateTime.Now;
                            message.NameFerma = client.name;
                            message.Priority = 1;

                            if (pingReply.Status != IPStatus.Success)
                                message.Text = "не подключена к сети!";
                            if (!client.connect)
                                message.Text = "app FermaMonitoring не запущена!";

                            

                            DeleteClient(client);

                            message.Text = message.Text + "\n" + "Количестов клиентов : " + listClient.Count;

                            listMessageToTelegram.Add(message);
                        }
                        else
                            i++;
                    }                    
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(DateTime.Now.ToString() + "error in FermaTelegram.TcpServer.client client" + ex.Message);
                    if (_del != null)
                        _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);
                }                

                Thread.Sleep(60*1000);
            }
        }

        public void SendCommand()
        {

            while (true)
            {
                try
                {
                    foreach (var command in listMessageFromTelegram.ToArray())
                    {                        
                        try
                        {
                            foreach (var client in listClient.ToArray())
                            {

                                FermaMessage message = new FermaMessage();

                                //message.NameFerma = name;
                                //message.Date = DateTime.Now;
                                message.Priority = 1;
                                message.Text = command;
                                message.Type = "command";

                                if (client.SendData(message) > 0)
                                    listMessageFromTelegram.Remove(command);
                            }
                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine(DateTime.Now.ToString() + "error in FermaTelegram.TcpServer.SendCommand command" + ex.Message);
                            if (_del != null)
                                _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);
                            break;
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(DateTime.Now.ToString() + "error in FermaTelegram.TcpServer.SendCommand client" + ex.Message);
                    if (_del != null)
                        _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);
                }
                Thread.Sleep(100);

            }
        }

        public void ReciveMessageFromClient()
        {
            while (true)
            {
                try
                {
                    foreach (var client in listClient.ToArray())
                    {
                        if (client != null)
                        {
                            try
                            {
                                foreach (var message in client.listMessage.ToArray())
                                {
                                    if (message != null)
                                    {                                                                            
                                        listMessageToTelegram.Add(message);
                                        //Console.WriteLine(DateTime.Now.ToString() + " message to Telegram : array listMessageToTelegram.count " + listMessageToTelegram.Count + " -- " + message);
                                        // добавить вызов метода для отправки сообщения в телеграмме

                                    }
                                    client.listMessage.Remove(message);
                                }
                            }
                            catch (Exception ex)
                            {
                                //Console.WriteLine(DateTime.Now.ToString() + "error in FermaTelegram.TcpServer.ReciveMessageFromClient message" + ex.Message);
                                if (_del != null)
                                    _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(DateTime.Now.ToString() + "error in FermaTelegram.TcpServer.ReciveMessageFromClient client" + ex.Message);
                    if (_del != null)
                        _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);
                }
                Thread.Sleep(100);
            }
        }

        private void DeleteClient(ClientObject client)
        {
            if (client != null)
            {
                client.cts_recive.Cancel();
                listClient.Remove(client);                
            }
        }

        private void AddIPClient(ClientObject client)
        {
            int i = 0;
            ClientObject clObj;
            while (i < listClient.Count)
            {
                clObj = listClient[i];
                if (clObj.IPAddr.Equals(client.IPAddr))
                    DeleteClient(clObj);
                else
                    i++;                
            }

            listClient.Add(client);
            Console.WriteLine("Client with IP = " + client.IPAddr + " and hostname = " + client.name);

            //FermaMessage message = new FermaMessage();
            //message.Date = DateTime.Now;
            //message.NameFerma = client.name;
            //message.Priority = 2;
            //message.Text = " в сети!";
            //listMessageToTelegram.Add(message);


        }

        public void GetNewClient()
        {

            while (true)
            {
                Console.WriteLine(DateTime.Now.ToString() + " ждем подключений от клиентов, всего клиентов - " + listClient.Count);
                Socket handler = listenSocket.Accept();

                IPAddress IPAddr = IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString());
                IPHostEntry hostInfo = Dns.GetHostEntry(IPAddr);

                ClientObject client = new ClientObject(handler, delayShowMessage, IPAddr, hostInfo);                                              

                AddIPClient(client);
                                                
                client.cts_recive = new CancellationTokenSource();

                Task reciveTask = Task.Run(() => client.Recieve(client.cts_recive.Token), client.cts_recive.Token);
                                
                client.recive = reciveTask;                                                                            

                Thread.Sleep(100);

                // закрываем сокет
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

            }
        }
    }     
        

}
