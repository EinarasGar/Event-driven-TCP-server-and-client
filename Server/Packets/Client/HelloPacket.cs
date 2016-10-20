using System;

namespace Server.Packets.Client
{
    class HelloPacket : Packet
    {
        public string Name;
        public string Password;
        public DateTime Time = DateTime.Now;

        public override PacketType Type
        { get { return PacketType.Hello; } }

        public override void Read(PacketReader r)
        {
            Name = r.ReadString();
            Password = r.ReadString();
            Time = DateTime.FromBinary(r.ReadInt64());
        }

        public override void Write(PacketWriter w)
        {
            w.Write(Name);
            w.Write(Password);
            w.Write(Time.ToBinary());
        }
    }
}
