using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using OpenHardwareMonitor.Hardware;

namespace FermaMonitoring
{
    class VideoCard
    {
        public string name;
        public string identifier;
        public List<string> listMessage;
        public int delayShowMessage;
        
        public ISensor senTemp;
        public ISensor senCont;
        public IHardware hardware;

        public float temperature;
        public float control;

        public float hiTemp = 70;
        public float loTemp = 25;
        public float loContTemp = 65;

        public float hiCont = 95;
        public float loCont = 0;

        public Task checkMaxTemp;
        public Task checkMinTemp;

        public Task checkMaxCont;
        public Task checkMinCont;

        public VideoCard()
        {
            listMessage = new List<string>();

            checkMaxTemp = new Task(TaskCheckMaxTemp);
            checkMinTemp = new Task(TaskCheckMinTemp);

            checkMaxCont = new Task(TaskCheckMaxCont);
            checkMinCont = new Task(TaskCheckMinCont);            
        }

        private void TaskCheckMaxTemp()
        {
            while (true)
            {
                if (temperature >= hiTemp)
                {
                    string message = name + " высокая температура " + "t = " + "*" + temperature + "*" + " C" + " обороты с = " + control + " %";
                    listMessage.Add(message);
                    Thread.Sleep(delayShowMessage);
                } 
                else
                    Thread.Sleep(100);
            }
        }

        private void TaskCheckMinTemp()
        {
            while (true)
            {
                if (temperature <= loTemp)
                {
                    string message = name + " низкая температура " + "t = " + "*" + temperature + "*" + " C" + " обороты с = " + control + " %";
                    listMessage.Add(message);
                    Thread.Sleep(delayShowMessage);
                }
                else
                    Thread.Sleep(100);
            }
        }

        private void TaskCheckMaxCont()
        {
            while (true)
            {
                if ((control >= hiCont) & (temperature >= hiTemp))
                {
                    string message = name + " высокие обороты " + "c = " + "*" + control + "*" + " %" + " температура t = " + temperature + " C";
                    listMessage.Add(message);
                    Thread.Sleep(delayShowMessage);
                }
                else
                    Thread.Sleep(100);
            }
        }

        private void TaskCheckMinCont()
        {
            while (true)
            {
                if ((control <= loCont) & (temperature >= loContTemp))
                {
                    string message = name + " низкие обороты " + "c = " + "*" + control + "*" + " %" + " температура t = " + temperature + " C";
                    listMessage.Add(message);
                    Thread.Sleep(delayShowMessage);
                }
                else
                    Thread.Sleep(100);
            }
        }

        public void UpDateSens()
        {
            hardware.Update();
            temperature = senTemp.Value.GetValueOrDefault();
            control = senCont.Value.GetValueOrDefault();
        }

    }        
}
