using GAFBot.MessageSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAFBot.Commands
{
    public class BaseCommand : ICommand
    {
        public virtual char Activator { get; set; }
        public virtual string CMD { get; set; }
        public virtual AccessLevel AccessLevel { get; set; }
        public int UseCount { get; private set; }

        public virtual void Activate(CommandEventArg e)
        {
            /*
             * ToDo: build area for logging
             */

            UseCount++;
        }
    }
}
