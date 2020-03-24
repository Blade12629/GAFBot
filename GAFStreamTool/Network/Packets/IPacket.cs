using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GAFStreamTool.Network.Packets
{
    public interface IPacket
    {
        byte Id { get; }
        void Handle(PacketReader reader, Client client);
        void Send(PacketWriter writer, Client client, params object[] data);
    }
}
