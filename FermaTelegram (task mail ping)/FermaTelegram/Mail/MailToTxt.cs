using OpenPop.Mime;
using OpenPop.Mime.Header;
using OpenPop.Pop3;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FermaTelegram
{
    class BalanceMessage
    {
        public DateTime date;
        public string text;
        public int hours;
        public double amount;
        public double price;
    }

    class MailToCSV
    {
        private string hostname;
        private int port;
        private bool useSsl;
        private string username;
        private string password;
        private string filename;
        public Task fetchMail;

        public delegate void SendErrorLog(string message);
        SendErrorLog _del;

        public void RegisterHandler(SendErrorLog del)
        {
            _del = del;
        }

        public MailToCSV(string hostname, int port, bool useSsl, string username, string password, string filename)
        {
            if (String.IsNullOrEmpty(hostname)) throw new Exception("Необходимо указать адрес pop3-сервера!");
            if (String.IsNullOrEmpty(username)) throw new Exception("Необходимо указать имя пользователя!");
            if (String.IsNullOrEmpty(password)) throw new Exception("Неоибходимо указать пароль пользователя!");

            if (port <= 0) port = 110;

            this.hostname = hostname;
            this.port = port;
            this.username = username;
            this.password = password;
            this.useSsl = useSsl;            

            string path = Directory.GetCurrentDirectory();
            this.filename = path + "\\" + filename;
            fetchMail = new Task(FetchBalanceMessage);
            fetchMail.Start();
        }

        private void SaveTxt(List<BalanceMessage> listBalanceMessage)
        {
            if (listBalanceMessage.Count > 0)
            {
                var txt = new StringBuilder();

                for (int i = 0; i < listBalanceMessage.Count; i++)
                {
                    string date = listBalanceMessage[i].date.ToString();
                    string amount = listBalanceMessage[i].amount.ToString();
                    string line = date + "\t" + amount;
                    txt.AppendLine(line);
                }
                if (File.Exists(filename))
                {
                    try
                    {
                        File.AppendAllText(filename, txt.ToString());
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine(ex.Message);
                        if (_del != null)
                            _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);
                    }
                }
                else
                {
                    File.WriteAllText(filename, txt.ToString());
                }
            }

        }

        public List<BalanceMessage> LoadTxt()
        {
            List<BalanceMessage> listBalanceTotal = new List<BalanceMessage>();

            if (File.Exists(filename))
            {
                List<BalanceMessage> listBalance = new List<BalanceMessage>();
                StreamReader fs = new StreamReader(filename);
                string s = "";
                while ((s = fs.ReadLine()) != null)
                {                    
                    int index = s.IndexOf("\t");
                    string date = s.Substring(0, index);
                    string amount = s.Substring(index + 1, s.Length - index - 1);
                    BalanceMessage bMessage = new BalanceMessage();

                    NumberFormatInfo provider = new NumberFormatInfo();
                    provider.NumberDecimalSeparator = ",";

                    bMessage.amount = Convert.ToDouble(amount, provider);
                    bMessage.date = Convert.ToDateTime(date);
                    listBalance.Add(bMessage);

                    //Console.WriteLine("from file = " + bMessage.date + " : " + bMessage.amount);
                }                

                double amountDay = 0;                

                int i = 0;

                DateTime date1 = listBalance[0].date;

                while  (i < listBalance.Count)
                {
                    
                    DateTime date2 = listBalance[i].date;
                    //double diff = (date2 - date1).TotalDays;

                    if ((date1.Year == date2.Year) && (date1.Month == date2.Month) && (date1.Day == date2.Day))
                    {
                        amountDay = amountDay + listBalance[i].amount;                        
                        i = i + 1;
                    }

                    DateTime curDate = DateTime.Now;

                    if (((date1.Year == date2.Year) && (date1.Month == date2.Month) && (date1.Day != date2.Day)) ||
                        ((date1.Year == date2.Year) && (date1.Month != date2.Month)) ||
                        ((date1.Year != date2.Year)) ||
                        i == listBalance.Count)
                    {
                        BalanceMessage bMessageTotal = new BalanceMessage();
                        bMessageTotal.date = Convert.ToDateTime(date1.ToShortDateString());

                        if ((curDate.Year == date1.Year) && (curDate.Month == date1.Month) && (curDate.Date == date1.Date))
                        {
                            bMessageTotal.hours = (curDate - date1).Hours;
                        }
                        else
                        {
                            bMessageTotal.hours = (listBalance[i - 1].date - date1).Hours;
                        }

                        bMessageTotal.amount = amountDay;
                        listBalanceTotal.Add(bMessageTotal);

                        if (i < listBalance.Count)
                        {
                            date1 = listBalance[i].date;                            
                        }

                        amountDay = 0;                        
                    }
                }                
            }
            else
            {
                //Console.WriteLine(filename + " не найден!");
                if (_del != null)
                    _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + " : " + filename + " не найден!");
            }

            return listBalanceTotal;
        }

        public void FetchBalanceMessage()
        {
            while (true)
            {
                int messageCount = 0;

                List<BalanceMessage> listBalanceMessage = new List<BalanceMessage>();
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
                        if (from.HasValidMailAddress && from.Address.Contains("noreply@suprnova.cc"))
                        {
                            DateTime date = Convert.ToDateTime(headers.Date);
                            Message message = client.GetMessage(i);
                            //MessagePart plainText = message.FindFirstPlainTextVersion();                    

                            BalanceMessage bMessage = new BalanceMessage();
                            bMessage.date = date;
                            bMessage.text = message.MessagePart.GetBodyAsText();

                            Regex regex = new Regex(@"\d\.\d+");
                            MatchCollection matches = regex.Matches(bMessage.text);

                            try
                            {
                                NumberFormatInfo provider = new NumberFormatInfo();
                                provider.NumberDecimalSeparator = ".";
                                double amount = Convert.ToDouble(matches[0].Value, provider);
                                bMessage.amount = amount;

                                //Console.WriteLine(bMessage.date + " : " + bMessage.amount);
                                listBalanceMessage.Add(bMessage);
                                client.DeleteMessage(i);
                            }
                            catch (Exception ex)
                            {
                                //Console.WriteLine(ex.Message);
                                if (_del != null)
                                    _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);
                                break;
                            }

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

                SaveTxt(listBalanceMessage);

                Thread.Sleep(1000 * 60 * 60);
            }
        }
    }
}
