namespace Server.Packets.Server
{
    class MessagePacket : Packet
    {
        public enum MessageTypes : byte
        {
            Normal,
            Warning,
            Announcement
        }

        public string String;
        public MessageTypes MessageType = MessageTypes.Normal; // If packet is being sent without specifying the type, it will alwaysa be "normal"

        public override PacketType Type
        { get { return PacketType.Message; } }

        public override void Read(PacketReader r)
        {
            MessageType = (MessageTypes)r.ReadByte();
            String = r.ReadString();
        }

        public override void Write(PacketWriter w)
        {
            w.Write((byte)MessageType);
            w.Write(String);
        }
    }
}