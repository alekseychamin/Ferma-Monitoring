using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FermaMonitoring
{
    abstract class Command
    {
        public string nameCommand;
        public Ferma ferma;

        public Command(string _name,Ferma _ferma)
        {
            nameCommand = _name;
            ferma = _ferma;
        }

        public abstract void Excecute();
    }

    class CommandMiner : Command
    {
        string nameMiner;
        public CommandMiner(string _nameCommand, Ferma _ferma, string _nameMiner)
            : base(_nameCommand, _ferma)
        {
            nameCommand = _nameCommand;
            ferma = _ferma;
            nameMiner = _nameMiner;
        }
        public override void Excecute()
        {
            if (ferma.currentMiner.name != nameMiner)
            {
                ferma.currentMiner.name = nameMiner;
                ferma.ChangeCurMiner();                
            }
            else
            {
                FermaMessage message = new FermaMessage();
                message.NameCommand = nameCommand;
                message.NameFerma = ferma.name;
                message.Date = DateTime.Now;
                message.Priority = 3;
                message.Text = nameMiner + " майнер уже выбран!";

                ferma.listMessage.Add(message);
            }

            if (!ferma.firststartMiner)
                ferma.StartCurrentMiner(firstStart: false);
            else
            {
                FermaMessage message = new FermaMessage();
                message.NameCommand = nameCommand;
                message.NameFerma = ferma.name;
                message.Date = DateTime.Now;
                message.Priority = 3;
                message.Text = nameMiner + " отложенный запуск майнера!";
                ferma.listMessage.Add(message);
            }

            ferma.command = "";
        }
    }

    class CommandStop : Command
    {
        public CommandStop(string _name, Ferma _ferma)
            : base(_name, _ferma)
        {
            nameCommand = _name;
            ferma = _ferma;
        }

        public override void Excecute()
        {
            ferma.StopMiner();
            FermaMessage message = new FermaMessage();
            message.NameCommand = nameCommand;
            message.NameFerma = ferma.name;
            message.Date = DateTime.Now;
            message.Priority = 3;
            message.Text = ferma.currentMiner.name + " майнер остановлен!";            
            ferma.listMessage.Add(message);

            ferma.command = "";
        }
    }

    class CommandTemp : Command
    {
        public CommandTemp(string _name, Ferma _ferma)
            : base(_name, _ferma)
        {
            nameCommand = _name;
            ferma = _ferma;
        }

        public override void Excecute()
        {
            ferma.UpDateGPUInformation();
            //ferma.DisplayGPUInformation();
            //ferma.tcpClient.SendData(ferma.hw.GetGPUFermaTemp());
            ferma.UpdateProcessMiner();

            FermaMessage message = new FermaMessage();
            message.NameCommand = nameCommand;
            message.NameFerma = ferma.name;
            message.Date = DateTime.Now;
            message.Priority = 3;
            message.Text = ferma.GetGPUFermaTemp();            
            ferma.listMessage.Add(message);

            ferma.command = "";
        }
    }

    class CommandMaxTemp : Command
    {
        public CommandMaxTemp(string _name, Ferma _ferma)
            : base(_name, _ferma)
        {
            nameCommand = _name;
            ferma = _ferma;
        }

        public override void Excecute()
        {
            ferma.UpDateGPUInformation();
            //ferma.DisplayGPUInformation();
            //ferma.tcpClient.SendData(ferma.hw.GetGPUFermaTemp());
            ferma.UpdateProcessMiner();

            FermaMessage message = new FermaMessage();
            message.NameCommand = nameCommand;
            message.NameFerma = ferma.name;
            message.Date = DateTime.Now;
            message.Priority = 3;
            message.Text = ferma.GetGPUFermaMaxTemp();
            ferma.listMessage.Add(message);

            ferma.command = "";
        }
    }

    class CommandCont : Command
    {
        public CommandCont(string _name, Ferma _ferma)
            : base(_name, _ferma)
        {
            nameCommand = _name;
            ferma = _ferma;
        }

        public override void Excecute()
        {
            ferma.UpDateGPUInformation();
            //ferma.DisplayGPUInformation();
            //ferma.tcpClient.SendData(ferma.hw.GetGPUFermaTemp());
            ferma.UpdateProcessMiner();

            FermaMessage message = new FermaMessage();
            message.NameCommand = nameCommand;
            message.NameFerma = ferma.name;
            message.Date = DateTime.Now;
            message.Priority = 3;
            message.Text = ferma.GetGPUFermaCont();
            ferma.listMessage.Add(message);

            ferma.command = "";
        }
    }
}
