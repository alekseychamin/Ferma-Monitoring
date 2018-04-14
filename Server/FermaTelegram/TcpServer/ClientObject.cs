using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FermaTelegram
{
    class ClientObject
    {
        private static string connectMessage = "online";

        private string clientMessage = "";
        private bool sendBusy;
        public bool connect = true;
        public int delayShowMessage;
        public Socket handler;
        public string IPAddr { get; set; }

        public string name = "";
        public List<FermaMessage> listMessage;

        public Task recive;
        
                
        public CancellationTokenSource cts_recive;
       
        public delegate void SendErrorLog(string message);
        SendErrorLog _del;

        public void RegisterHandler(SendErrorLog del)
        {
            _del = del;
        }


        public ClientObject(Socket _handler, int _delay, IPAddress IPAddr, IPHostEntry hostInfo)
        {
            handler = _handler;            
            connect = true;
            listMessage = new List<FermaMessage>();
            delayShowMessage = _delay;

            this.IPAddr = IPAddr.ToString();
            this.name = hostInfo.HostName;

        }               
                

        public void Recieve(CancellationToken cancellationToken)
        {
            // получаем сообщение
            while (true)
            {
                if (connect)
                {
                    try
                    {
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0; // количество полученных байтов
                        byte[] data = new byte[4096]; // буфер для получаемых данных

                        do
                        {
                            bytes = handler.Receive(data, data.Length, 0);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                            Console.WriteLine(DateTime.Now.ToString() + " Telegram.ClientRecive: " + data);                            

                        } while (handler.Available > 0);

                        string json = builder.ToString();
                        

                        if (json != "")
                        {
                            //Console.WriteLine(DateTime.Now.ToShortTimeString() + ": recieved from client " + builder.ToString());


                            //clientMessage = connectMessage;

                            //connect = true;

                            FermaMessage message = JsonConvert.DeserializeObject<FermaMessage>(json);
                           

                            if (message.Type != "system")
                            {
                                listMessage.Add(message);
                                
                            }
                            else
                            {
                                if (message.Text.Contains(connectMessage))
                                    clientMessage = connectMessage;
                            }

                        }
                        else
                        {
                            //connect = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine(DateTime.Now.ToString() + " :" + " Error in ReciveFromClient TcpServer : " + ex.Message);
                        if (_del != null)
                            _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);
                        connect = false;
                        Thread.Sleep(5000);
                    }
                }

                Thread.Sleep(100);

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (Exception ex)
                {
                    break;
                }
            }
        }

        public int SendData(FermaMessage message)
        {
            int result = 0;

            if ((connect) && (message != null) && (!sendBusy))
            {
                sendBusy = true;
                string serialized = JsonConvert.SerializeObject(message);
                try
                {
                    byte[] data = Encoding.Unicode.GetBytes(serialized);
                    result = handler.Send(data);
                    //Console.WriteLine(DateTime.Now.ToString() + " Telegram.SendCommand: " + message);
                    Thread.Sleep(50);

                }
                catch (Exception ex)
                {
                    //Console.WriteLine(DateTime.Now.ToString() + " :" + " Error in SendData : " + ex.Message);
                    if (_del != null)
                        _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);
                    connect = false;
                }
                sendBusy = false;
            }
            else
            {
                //Console.WriteLine("не возможно отправить " + message);
                if (_del != null)
                    _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + " : " + " не возможно отправить " + message);
            }

            return result;
        }        

    }    
}
