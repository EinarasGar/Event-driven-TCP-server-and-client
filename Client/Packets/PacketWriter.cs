using System.IO;

namespace Client.Packets
{
    public class PacketWriter : BinaryWriter
    {
        private MemoryStream _ms;

        public PacketWriter()
            : base()
        {
            _ms = new MemoryStream();
            OutStream = _ms;
        }

        public byte[] GetBytes()
        {
            Close();

            byte[] data = _ms.ToArray();
            return data;
        }
    }

    public class PacketReader : BinaryReader
    {
        byte[] data;
        public PacketReader(byte[] data)
            : base(new MemoryStream(data))
        {
            this.data = data;

        }
    }
}
