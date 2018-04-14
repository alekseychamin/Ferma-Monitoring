using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading;

namespace FermaTelegram
{
    class TelegramMessage
    {
        public string reciveMessage;
        public FermaMessage telegramMessage;
    }

    class TelegramBot
    {
        public string token;
        public TelegramBotClient Bot;
        public bool start = false;
        public long ChatId = 0;
        public long ChatIdWarning = 0;

        public List<string> listMessageToClient;
        public List<FermaMessage> listMessageFromClient;
        public List<string> listMessageToApp;

        public Task upMessage;
        public Task sendMessage;

        public delegate void SendErrorLog(string message);
        SendErrorLog _del;

        public void RegisterHandler(SendErrorLog del)
        {
            _del = del;
        }

        public TelegramBot(string _token, List<string> _listMessageToClient, List<FermaMessage> _listMessageFromClient)
        {
            token = _token;

            if (_listMessageFromClient != null)
                listMessageFromClient = _listMessageFromClient;

            if (_listMessageToClient != null)
                listMessageToClient = _listMessageToClient;

            listMessageToApp = new List<string>();

            upMessage = new Task(UpTelegramMessage);
            upMessage.Start();

            sendMessage = new Task(SendMessage);
            sendMessage.Start();
        }

        private async void UpTelegramMessage()
        {
            //throw new NotImplementedException();                                                

            try
            {
                Bot = new Telegram.Bot.TelegramBotClient(token);
                await Bot.SetWebhookAsync("");
                start = true;
                Console.WriteLine("Telegram Bot запущен " + Bot.ToString());

            }

            catch (Exception ex)
            {
                //Console.WriteLine(DateTime.Now.ToString() + "\n" + "Invalid Token KEY: " + ex.Message); // если ключ не подошел - пишем об этом в консоль отладки                 
                if (_del != null)
                    _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);

                start = false;
            }

            if (start & Bot != null)
            {
                UpMessage();
                
            }

        }

        private async void UpMessage()
        {
            int offset = 0;            

            while (true)
            {
                // цикл по всем фермаа для определения необходимости выводить предупреждения                                                                    

                try
                {

                    var updates = await Bot.GetUpdatesAsync(offset);

                    foreach (var update in updates)
                    {
                        var message = update.Message;
                        if (message != null)
                        {
                            ChatId = message.Chat.Id;

                            if (message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                            {
                                ChatId = message.Chat.Id;
                                listMessageToClient.Add(message.Text);
                                listMessageToApp.Add(message.Text);
                                //Console.WriteLine(DateTime.Now.ToString() + "recive message from Telegram: listMessageToClient.Count " + listMessageToClient.Count + " -- " + message.Text);
                                
                            }
                        }
                        offset = update.Id + 1;
                    }
                    
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(DateTime.Now.ToString() + "\n" + "Error in Bot Do Work get update: " + ex.Message);
                    if (_del != null)
                        _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);

                }                                
            }

        }

        public async void SendMessage()
        {
            while (true)
            {
                try
                {
                    foreach (var telmessage in listMessageFromClient.ToArray())
                    {
                        if (telmessage != null)
                        {
                            string text = "";
                            long localChatId = ChatId;

                            if (telmessage.Priority == 3)
                            {
                                text = telmessage.NameCommand + "\n" + telmessage.Date.ToString() + "\n" + "*" + telmessage.NameFerma + "*" + "\n" +
                                        telmessage.Text;                                
                            }

                            if (telmessage.Priority != 3)
                            {
                                text = telmessage.Date.ToString() + "\n" + "*" + telmessage.NameFerma + "*" + "\n" +
                                        telmessage.Text;
                                localChatId = ChatIdWarning;
                            }

                            try
                            {
                                await Bot.SendTextMessageAsync(localChatId, text, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                //Console.WriteLine(DateTime.Now.ToString() + ": Chat ID " + ChatId);
                                listMessageFromClient.Remove(telmessage);
                            }
                            catch (Exception ex)
                            {
                                //Console.WriteLine(DateTime.Now.ToString() + "\n" + "Error in Bot Send Message: " + ex.Message);
                                if (_del != null)
                                    _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(DateTime.Now.ToString() + "\n" + "Error in Bot Send Message: " + ex.Message);
                    if (_del != null)
                        _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);
                }
                Thread.Sleep(100);
            }
        }
    }
}
