using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FermaTelegram
{
    class ParserJson
    {
        static int max = 1440;
        private double[] hashrate = new double[max];
        private double[] sharerate = new double[max];
        private int index = 0;
        private string url;
        private double averHashrate;
        private double averSharerate;
        private double curHashrate;
        private double curSharerate;

        public delegate void SendErrorLog(string message);
        SendErrorLog _del;

        public void RegisterHandler(SendErrorLog del)
        {
            _del = del;
        }

        public double CurHashrate
        {
            get
            {
                return curHashrate;
            }
        }

        public double CurSharerate
        {
            get
            {
                return curSharerate;
            }
        }

        public double AverHashrate
        {
            get
            {
                return averHashrate;
            }
        }
        public double AverSharerate
        {
            get
            {
                return averSharerate;
            }
        }


        public ParserJson(string url)
        {
            this.url = url;
        }

        private double averCalculate(double[] array)
        {
            double aver = 0;

            for (int i = 0; i < max; i++)
            {
                aver = aver + array[i];
            }

            aver = aver / max;

            return aver;
        }

        public void ParseJson()
        {
            while (true)
            {
                WebClient webClient = new WebClient();

                double hash = 0;
                double share = 0;

                try
                {
                    string response = webClient.DownloadString(url);
                    dynamic obj = JsonConvert.DeserializeObject(response);

                    hash = obj.getuserstatus.data.hashrate;
                    share = obj.getuserstatus.data.sharerate;

                    curHashrate = hash / 1000;
                    curSharerate = share;
                }
                catch (Exception ex)
                {
                    if (_del != null)
                        _del(ex.Message);
                }

                


                if (index < max)
                {
                    hashrate[index] = hash;
                    sharerate[index] = share;
                    index++;
                }
                else
                {
                    index = 0;                    
                }

                averHashrate = averCalculate(hashrate)/1000;
                averSharerate = averCalculate(sharerate);

                //Console.WriteLine("ParseJson : " + "index = " + index + " hash = " + hash);
                //if (_del != null)
                //    _del("ParseJson : " + "index = " + index + " hash = " + hash);

                Thread.Sleep(1000 * 60);
            }
        }

    }
}
