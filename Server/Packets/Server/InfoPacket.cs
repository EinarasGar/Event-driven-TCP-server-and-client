using System;

namespace Server.Packets.Server
{
    class InfoPacket : Packet
    {
        public DateTime Time;
        public int ConnectedClients;
        public string Message;

        public override PacketType Type
        { get { return PacketType.Info; } }

        public override void Read(PacketReader r)
        {
            ConnectedClients = r.ReadInt32();
            Message = r.ReadString();
            Time = DateTime.FromBinary(r.ReadInt64());
        }

        public override void Write(PacketWriter w)
        {
            w.Write(ConnectedClients);
            w.Write(Message);
            w.Write(Time.ToBinary());
        }
    }
}