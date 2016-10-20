namespace Client.Packets.Server
{
    class MessagePacket : Packet
    {
        public enum MessageTypes : byte
        {
            Normal,
            Warning,
            Announcement
        }

        public string Message;
        public MessageTypes MessageType = MessageTypes.Normal;

        public override PacketType Type
        { get { return PacketType.Message; } }

        public override void Read(PacketReader r)
        {
            MessageType = (MessageTypes)r.ReadByte();
            Message = r.ReadString();
        }

        public override void Write(PacketWriter w)
        {
            w.Write((byte)MessageType);
            w.Write(Message);
        }
    }
}
