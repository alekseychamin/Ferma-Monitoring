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
        public float loTemp = 35;
        public float loContTemp = 65;

        public float hiCont = 95;
        public float loCont = 0;

        private bool isHighTemp;
        private bool isLowTemp;

        private bool isHighCont;
        private bool isLowCont;

        public VideoCard()
        {
            listMessage = new List<string>();

            
        }

        public void TaskCheckMaxTemp()
        {
            if (temperature >= hiTemp)
            {
                string message = name + " высокая температура " + "t = " + temperature + " C" + " обороты с = " + control + " %";
                listMessage.Add(message);                
            }                        
        }

        public void TaskCheckMinTemp()
        {
            if (temperature <= loTemp)
            {
                string message = name + " низкая температура " + "t = " + temperature + " C" + " обороты с = " + control + " %";
                listMessage.Add(message);
            }                        
        }

        public void TaskCheckMaxCont()
        {
            if ((control >= hiCont) & (temperature >= hiTemp))
            {
                string message = name + " высокие обороты " + "c = " + control + " %" + " температура t = " + temperature + " C";
                listMessage.Add(message);
            }                                                        
        }

        public void TaskCheckMinCont()
        {
            if ((control <= loCont) & (temperature >= loContTemp))
            {
                string message = name + " низкие обороты " + "c = " + control + " %" + " температура t = " + temperature + " C";
                listMessage.Add(message);             
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
