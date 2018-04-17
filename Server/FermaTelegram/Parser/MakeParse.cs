using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FermaTelegram
{
    class MakeParse
    {
        public ParserHtml parserZec;
        public ParserHtml parserZcl;
        public ParserHtml parserEth;
        public ParserJson calcHashShare;
        private TelegramBot telegramBot;        
        private MailCommand mail;

        public delegate void SendErrorLog(string message);
        SendErrorLog _del;

        public void RegisterHandler(SendErrorLog del)
        {
            _del = del;
        }

        public string resultParser;

        public MakeParse(TelegramBot telegramBot, MailCommand mail)
        {
            parserZec = new ParserHtml();
            parserZcl = new ParserHtml();
            parserEth = new ParserHtml();

            this.telegramBot = telegramBot;
            this.mail = mail;
            calcHashShare = new ParserJson("https://zcl.suprnova.cc/index.php?page=api&action=getuserstatus&api_key=47f6d0c30fc8137556a02db32a450978c016ad5514dfa1deb7def6b1d73aa198&id=201016583");
        }

        public void TaskParseZec(string name)
        {
            //var task = Task.Run(() => ParseZec(name));            
            ParseZec(name);
        }

        public void TaskParseEth(string name)
        {
            ParseEth(name);
        }

        public void TaskParseZcl(string name)
        {
            var task = Task.Run(() => ParseZcl(name));
        }

        public async void ParseZcl(string name)
        {
            parserZcl.URL = "https://ratesviewer.com/chart/zcl-usdt/year/";
            await parserZcl.MakeDocumentHtmlAsync();
            string sUSD = parserZcl.ParseBySelector("body > div.container > div > div > div:nth-child(6) > div.col-sm-3 > div > div:nth-child(1) > div.value");

            int index = sUSD.IndexOf("T");
            string sZclUSD = sUSD.Substring(index + 2, sUSD.Length - index - 2);

            //Console.WriteLine(courseUSD);

            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";



            double dZclUSD = Convert.ToDouble(sZclUSD, provider);

            List<BalanceMessage> listBalanceTotal = new List<BalanceMessage>();

            listBalanceTotal = mail.LoadTxt();

            string res = 
                         "Курс ZCL/$ : " + "*" + sZclUSD + "*" + "\n" +
                         "Текущая скорость : " + "*" + calcHashShare.CurHashrate.ToString("0.0") + " has/s" + "*" + "\n" +
                         "Текущее кол-во шар : " + "*" + calcHashShare.CurSharerate.ToString("0") + " шар" + "*" + "\n" + 
                         "\n" +
                         "Средняя скорость : " + "*" + calcHashShare.AverHashrate.ToString("0.0") + " has/s" + "*" + "\n" +
                         "Среднее кол-во шар : " + "*" + calcHashShare.AverSharerate.ToString("0") + " шар" + "*" + "\n" + 
                         "\n";

            foreach (var message in listBalanceTotal)
            {
                message.price = message.amount * dZclUSD;
                res = res + message.date.ToString("dd.MM.yy") + " : " + 
                            "*" + message.amount.ToString("0.00") + " ZCL" + "*" + " : " + 
                            "*" + message.price.ToString("0.0") + " $ - "  + "*" +  message.hours + " ч" + "\n";
                //Console.WriteLine(message.date.ToShortDateString() + " : " + message.price);
            }

            DateTime curDate = DateTime.Now;
            double calcUSD = 0;
            double calcZCL = 0;

            if ((curDate.Date == listBalanceTotal[listBalanceTotal.Count - 1].date) && (listBalanceTotal[listBalanceTotal.Count - 1].hours != 0))
            {

                calcUSD = (listBalanceTotal[listBalanceTotal.Count - 1].amount / listBalanceTotal[listBalanceTotal.Count - 1].hours) * 24 * dZclUSD;
                calcZCL = (listBalanceTotal[listBalanceTotal.Count - 1].amount / listBalanceTotal[listBalanceTotal.Count - 1].hours) * 24;
            }

            res = res + "\n" + 
                        "Расчет на 24 ч : " + calcZCL.ToString("0.0000") + " ZCL" + "\n" +
                        "Расчет на 24 ч : " + calcUSD.ToString("0.0") + " $";

            //Console.WriteLine(res);

            FermaMessage mes = new FermaMessage();
            mes.NameCommand = name;
            mes.NameFerma = "Telegram";
            mes.Date = DateTime.Now;
            mes.Priority = 3;
            mes.Text = res;
            

            telegramBot.listMessageFromClient.Add(mes);
        }

        public void ParseZec(string name)
        {
            try
            {
                WebClient webClient = new WebClient();
                parserZec.URL = "https://api-zcash.flypool.org/miner/:t1awFddn1dam2Vj5h3tz2BXcivN1o5j4irn/currentStats";

                string response = webClient.DownloadString(parserZec.URL);
                dynamic obj = JsonConvert.DeserializeObject(response);

                double currentHashrate = obj.data.currentHashrate / 1000;
                double averageHashrate = obj.data.averageHashrate / 1000;
                double unpaid = obj.data.unpaid / Math.Pow(10, 8);

                double coinsPerMin = obj.data.coinsPerMin;
                double usdPerMin = obj.data.usdPerMin;



                double paidUSD = usdPerMin * 60 * 24;
                double paidZEC = coinsPerMin * 60 * 24;

                double usdMounthPaid = paidUSD * 30;
                double coinsMounthPaid = paidZEC * 30;


                double course = 0;
                if (paidZEC != 0)
                {
                    //Console.WriteLine("PaidUSD =" + paidUSD + " paidZEC = " + paidZEC);
                    course = paidUSD / paidZEC;
                }

                parserZec.URL = "https://api-zcash.flypool.org/miner/:t1awFddn1dam2Vj5h3tz2BXcivN1o5j4irn/workers";

                response = webClient.DownloadString(parserZec.URL);
                obj = JsonConvert.DeserializeObject(response);

                double currentHashrate1 = obj.data[0].currentHashrate;
                double currentHashrate2 = obj.data[1].currentHashrate;
                double currentHashrate3 = obj.data[2].currentHashrate;

                currentHashrate1 = currentHashrate1 / 1000;
                currentHashrate2 = currentHashrate2 / 1000;
                currentHashrate3 = currentHashrate3 / 1000;

                string res =
                             "Текущая скорость  = " + "*" + currentHashrate.ToString("0.00") + "kH/s" + "*" + "\n" +
                             "Средняя скорость = " + "*" + averageHashrate.ToString("0.00") + "kH/s" + "*" + "\n" +
                             "Текущая скорость ferma1 = " + currentHashrate1.ToString("0.00") + "kH/s" + "\n" +
                             "Текущая скорость ferma2 = " + currentHashrate2.ToString("0.00") + "kH/s" + "\n" +
                             "Текущая скорость ferma3 = " + currentHashrate3.ToString("0.00") + "kH/s" + "\n" +
                             "Невыплаченный баланс = " + "*" + unpaid.ToString("0.000") + "ZEC" + "*" + "\n" +
                             "Заработок за день = " + "*" + paidUSD.ToString("0.00") + "$" + "*" + "/" + "*" + paidZEC.ToString("0.00") + "*" + "ZEC" + "\n" +
                             "Заработок в месяц = " + "*" + usdMounthPaid.ToString("0") + "$" + "*" + "/" + "*" + coinsMounthPaid.ToString("0.00") + "*" + "ZEC" + "\n" +
                             "Расчетный курс ZEC/USD = " + "*" + course.ToString("0") + "*" + "$";

                //Console.WriteLine(res);

                FermaMessage mes = new FermaMessage();
                mes.NameCommand = name;
                mes.NameFerma = "Telegram";
                mes.Date = DateTime.Now;
                mes.Priority = 3;
                mes.Text = res;

                telegramBot.listMessageFromClient.Add(mes);
            }
            catch (Exception ex)
            {
                if (_del != null)
                    _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);
            }
        }

        public void ParseEth(string name)
        {
            WebClient webClient = new WebClient();
            parserEth.URL = "https://api.ethermine.org/miner/:c0e96814bc0e8916988bab6f558786177fb2a424/currentStats";

            string response = webClient.DownloadString(parserEth.URL);
            dynamic obj = JsonConvert.DeserializeObject(response);

            double currentHashrate = obj.data.currentHashrate / Math.Pow(10, 6);
            double averageHashrate = obj.data.averageHashrate / Math.Pow(10, 6);
            //double unpaid = obj.data.unpaid / Math.Pow(10, 8);

            double coinsPerMin = obj.data.coinsPerMin;
            double usdPerMin = obj.data.usdPerMin;



            double paidUSD = usdPerMin * 60 * 24;
            double paidZEC = coinsPerMin * 60 * 24;

            double usdMounthPaid = paidUSD * 30;
            double coinsMounthPaid = paidZEC * 30;


            double course = 0;
            if (paidZEC != 0)
            {
                //Console.WriteLine("PaidUSD =" + paidUSD + " paidZEC = " + paidZEC);
                course = paidUSD / paidZEC;
            }

            parserEth.URL = "https://api.ethermine.org/miner/:c0e96814bc0e8916988bab6f558786177fb2a424/workers";

            response = webClient.DownloadString(parserEth.URL);
            obj = JsonConvert.DeserializeObject(response);

            double currentHashrate1 = obj.data[0].currentHashrate;
            double currentHashrate2 = obj.data[1].currentHashrate;
            double currentHashrate3 = obj.data[2].currentHashrate;

            currentHashrate1 = currentHashrate1 / Math.Pow(10, 6);
            currentHashrate2 = currentHashrate2 / Math.Pow(10, 6);
            currentHashrate3 = currentHashrate3 / Math.Pow(10, 6);

            string res =
                         "Текущая скорость  = " + "*" + currentHashrate.ToString("0.00") + " MH/s" + "*" + "\n" +
                         "Средняя скорость = " + "*" + averageHashrate.ToString("0.00") + " MH/s" + "*" + "\n" +
                         "Текущая скорость ferma 1 = " + currentHashrate1.ToString("0.00") + " MH/s" + "\n" +
                         "Текущая скорость ferma 2 = " + currentHashrate2.ToString("0.00") + " MH/s" + "\n" +
                         "Текущая скорость ferma 3 = " + currentHashrate3.ToString("0.00") + " MH/s" + "\n" +
                         //"Невыплаченный баланс = " + "*" + unpaid.ToString("0.00000") + " ZEC" + "*" + "\n" +
                         "Заработок за день = " + "*" + paidUSD.ToString("0.00") + "$" + "*" + "/" + "*" + paidZEC.ToString("0.0000") + "*" + " ETH" + "\n" +
                         "Заработок в месяц = " + "*" + usdMounthPaid.ToString("0.00") + "$" + "*" + "/" + "*" + coinsMounthPaid.ToString("0.00") + "*" + " ETH" + "\n" +
                         "Расчетный курс ETH/USD = " + "*" + course.ToString("0.0") + "*" + "$";

            //Console.WriteLine(res);

            FermaMessage mes = new FermaMessage();
            mes.NameCommand = name;
            mes.NameFerma = "Telegram";
            mes.Date = DateTime.Now;
            mes.Priority = 3;
            mes.Text = res;

            telegramBot.listMessageFromClient.Add(mes);
        }
    }
}
