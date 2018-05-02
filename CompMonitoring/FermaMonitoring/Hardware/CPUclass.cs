using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;

namespace FermaMonitoring
{
    class CPUclass
    {
        public string name;
        public List<ISensor> listSenTemp = new List<ISensor>();
        public IHardware hardware;        

        public void UpDateSens()
        {
            hardware.Update();            
        }
    }
}
