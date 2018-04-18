﻿using OpenPop.Mime;
using OpenPop.Mime.Header;
using OpenPop.Pop3;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FermaTelegram
{    
    class MailCommand
    {
        private string hostname;
        private int port;
        private bool useSsl;
        private string username;
        private string password;
        private string filename;
        private string fermaEmailAddr = "fermaalnik@gmail.com";
        private List<string> fromEmailAddr = new List<string>();
        private string alertEmailAddr;
        public ListMessage listMessage;
        public Task fetchMailCommand;
        public Task sendMailToClient;

        public delegate void SendErrorLog(string message);
        SendErrorLog _del;

        public void RegisterHandler(SendErrorLog del)
        {
            _del = del;
        }

        public MailCommand(string hostname, int port, bool useSsl, string username, string password)
        {
            if (String.IsNullOrEmpty(hostname)) throw new Exception("Необходимо указать адрес pop3-сервера!");
            if (String.IsNullOrEmpty(username)) throw new Exception("Необходимо указать имя пользователя!");
            if (String.IsNullOrEmpty(password)) throw new Exception("Необходимо указать пароль пользователя!");

            if (port <= 0) port = 110;

            this.hostname = hostname;
            this.port = port;
            this.username = username;
            this.password = password;
            this.useSsl = useSsl;            
           
            fetchMailCommand = new Task(FetchMailCommand);
            fetchMailCommand.Start();
        }        

        public void FetchMailCommand()
        {
            while (true)
            {
                int messageCount = 0;
                
                Pop3Client client = new Pop3Client();
                try
                {
                    client.Connect(hostname, port, useSsl);
                    client.Authenticate(username, password);

                    messageCount = client.GetMessageCount();

                    for (int i = 1; i <= messageCount; i++)
                    {
                        MessageHeader headers = client.GetMessageHeaders(i);
                        RfcMailAddress from = headers.From;
                        if (from.HasValidMailAddress && headers.Subject.Contains("command"))
                        {
                            DateTime date = Convert.ToDateTime(headers.Date);
                            Message message = client.GetMessage(i);
                            fromEmailAddr.Add(from.MailAddress.ToString());
                            //MessagePart plainText = message.FindFirstPlainTextVersion();                                                                            
                            listMessage.command.Add(message.MessagePart.GetBodyAsText());
                            client.DeleteMessage(i);
                        }
                    }
                    client.Disconnect();
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                    if (_del != null)
                        _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);
                }
                client.Dispose();
                SendMailReply();

                Thread.Sleep(1000 * 30 );
            }
        }

        public void SendMailReply()
        {
            while (true)
            {
                int i = 0;
                while (i < listMessage.reply.Count)
                {
                    FermaMessage message = listMessage.reply[i];

                    if (message.Priority == 3)
                    {
                        int j = 0;
                        while (j < fromEmailAddr.Count)
                        {
                            SmtpClient c = new SmtpClient("smtp.gmail.com", 587);
                            MailAddress add = new MailAddress(fromEmailAddr[j]);
                            MailMessage msg = new MailMessage();
                            msg.To.Add(add);
                            msg.From = new MailAddress(fermaEmailAddr);
                            msg.IsBodyHtml = true;
                            msg.Subject = listMessage.reply[i].NameCommand;
                            msg.Body = listMessage.reply[i].Text;
                            c.Credentials = new System.Net.NetworkCredential(fromEmailAddr[j], password);
                            c.EnableSsl = true;
                            c.Send(msg);
                            fromEmailAddr.RemoveAt(j);
                        }
                    }
                    else
                    {
                        SmtpClient c = new SmtpClient("smtp.gmail.com", 587);
                        MailAddress add = new MailAddress(alertEmailAddr);
                        MailMessage msg = new MailMessage();
                        msg.To.Add(add);
                        msg.From = new MailAddress(fermaEmailAddr);
                        msg.IsBodyHtml = true;
                        msg.Subject = listMessage.reply[i].NameFerma + listMessage.reply[i].NameCommand;
                        msg.Body = listMessage.reply[i].Text;
                        c.Credentials = new System.Net.NetworkCredential(alertEmailAddr, password);
                        c.EnableSsl = true;
                        c.Send(msg);
                    }

                    listMessage.reply.Remove(message);
                }

                Thread.Sleep(1000);
            }
        }
    }
}
