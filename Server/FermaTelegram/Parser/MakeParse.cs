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
        private ListMessage listMessage;                

        public delegate void SendErrorLog(string message);
        SendErrorLog _del;

        public void RegisterHandler(SendErrorLog del)
        {
            _del = del;
        }

        public string resultParser;

        public MakeParse(ListMessage listMessage)
        {                               
            this.listMessage = listMessage;            
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
               
        public void ParseZec(string name)
        {
            try
            {
                string URL = "https://api-zcash.flypool.org/miner/:t1awFddn1dam2Vj5h3tz2BXcivN1o5j4irn/currentStats";

                WebClient webClient = new WebClient();              
                string response = webClient.DownloadString(URL);
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

                URL = "https://api-zcash.flypool.org/miner/:t1awFddn1dam2Vj5h3tz2BXcivN1o5j4irn/workers";

                response = webClient.DownloadString(URL);
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

                listMessage.listMessageFromClient.Add(mes);
            }
            catch (Exception ex)
            {
                if (_del != null)
                    _del(this.GetType().ToString() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);
            }
        }

        public void ParseEth(string name)
        {
            string URL = "https://api.ethermine.org/miner/:c0e96814bc0e8916988bab6f558786177fb2a424/currentStats";
            WebClient webClient = new WebClient();            

            string response = webClient.DownloadString(URL);
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

            URL = "https://api.ethermine.org/miner/:c0e96814bc0e8916988bab6f558786177fb2a424/workers";

            response = webClient.DownloadString(URL);
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

            listMessage.FromClient.Add(mes);
        }
    }
}
