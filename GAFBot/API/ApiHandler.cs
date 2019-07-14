using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.API
{
    public interface ApiHandler
    {
        int VersionMajor { get; }
        int VersionMinor { get; }
        int[] Version { get; }

        string Name { get; }

        
        
    }
}
