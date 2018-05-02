using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus;
using Modbus.Device;
using Modbus.Data;
using Modbus.Message;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FermaMonitoring
{
    class ModbusTCP
    {
        private Ferma ferma;
        private int port = 502;
        private byte slaveId = 1;
        private bool isListen;
        private string IPaddress;        

        TcpListener slaveTcpListener;
        ModbusSlave slave;        
        

        public ModbusTCP(Ferma _ferma, string IPaddress)
        {
            ferma = _ferma;
            this.IPaddress = IPaddress;
            isListen = false;            
        }
              
       
        public void InitServer()
        {
            try
            {
                IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
                //IPAddress[] addr = ipEntry.AddressList;
                IPAddress IPaddr = IPAddress.Parse(IPaddress);

                //IPAddress address = new IPAddress(new byte[] { 192, 168, 0, 33 });

                // create and start the TCP slave
                slaveTcpListener = new TcpListener(IPaddr, port);
                slaveTcpListener.Start();

                slave = ModbusTcpSlave.CreateTcp(slaveId, slaveTcpListener);
                slave.DataStore = DataStoreFactory.CreateDefaultDataStore();
                slave.ModbusSlaveRequestReceived += new EventHandler<ModbusSlaveRequestEventArgs>(Modbus_Request_Event);                

                //slave.ListenAsync().GetAwaiter().GetResult();                      
                slave.ListenAsync();
                Console.WriteLine(DateTime.Now.ToString() + " ModbusTCP/IP сервер запущен на адресе : " + IPaddress);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString() + " Error in InitServer : " + ex.Message);
                isListen = false;
                
                slaveTcpListener.Stop();
                slaveTcpListener = null;
                slave.Dispose();      
            }
        }

        private void Modbus_Request_Event(object sender, Modbus.Device.ModbusSlaveRequestEventArgs e)
        {
            //request from master//disassemble packet from master
            byte fc = e.Message.FunctionCode;
            byte[] data = e.Message.MessageFrame;
            byte[] byteStartAddress = new byte[] { data[3], data[2] };
            byte[] byteNum = new byte[] { data[5], data[4] };
            Int16 StartAddress = BitConverter.ToInt16(byteStartAddress, 0);
            Int16 NumOfPoint = BitConverter.ToInt16(byteNum, 0);

            //Console.WriteLine(fc.ToString() + "," + StartAddress.ToString() + "," + NumOfPoint.ToString());           
     
        }

        public void SendData()
        {

            if (!isListen)
            {
                InitServer();
                isListen = true;
            }
            
            int i = 0;
            try
            {                                              
                foreach (var videocard in ferma.listVideoCard)
                {
                    i = i + 1;
                    slave.DataStore.HoldingRegisters[i] = Convert.ToUInt16(videocard.temperature);
                    slave.DataStore.HoldingRegisters[ferma.listVideoCard.Count + i] = Convert.ToUInt16(videocard.control);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString() + " Error in SendData : " + ex.Message);
                isListen = false;
                
                slaveTcpListener.Stop();
                slaveTcpListener = null;                
                slave.Dispose();             
            }
        }
    }
}
