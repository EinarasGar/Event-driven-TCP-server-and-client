namespace Client.Packets.Server
{
    class FailurePacket : Packet
    {
        public int ErrorId;
        public string ErrorMessage;

        public override PacketType Type
        { get { return PacketType.Failure; } }

        public override void Read(PacketReader r)
        {
            ErrorId = r.ReadInt32();
            ErrorMessage = r.ReadString();
        }

        public override void Write(PacketWriter w)
        {
            w.Write(ErrorId);
            w.Write(ErrorMessage);
        }
    }
}
