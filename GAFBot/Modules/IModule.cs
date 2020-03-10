using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Modules
{
    public interface IModule
    {
        bool Enabled { get; set; }
        string ModuleName { get; }

        void Enable();
        void Disable();
        void Initialize();
        void Dispose();
    }
}
