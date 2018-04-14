using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FermaTelegram
{
    abstract class Command
    {
        public string name;       

        public string Name
        {
            get { return name; }
        }

        public Command(string _name)
        {
            name = _name;            
        }

        public abstract void Excecute();
    }

    class CommandStatusZec : Command
    {
        private MakeParse makeParser;        

        public CommandStatusZec(string _name, MakeParse makeParser) : base(_name)
        {
            name = _name;
            this.makeParser = makeParser;            
        }

        public override void Excecute()
        {
            makeParser.TaskParseZec(name);            
        }

    }

    class CommandStatusEth : Command
    {
        private MakeParse makeParser;

        public CommandStatusEth(string _name, MakeParse makeParser) : base(_name)
        {
            name = _name;
            this.makeParser = makeParser;
        }

        public override void Excecute()
        {
            makeParser.TaskParseEth(name);
        }

    }

    class CommandStatusZcl : Command
    {
        private MakeParse makeParser;

        public CommandStatusZcl(string _name, MakeParse makeParser) : base(_name)
        {
            name = _name;
            this.makeParser = makeParser;
        }

        public override void Excecute()
        {
            makeParser.TaskParseZcl(name);
        }

    }

}
