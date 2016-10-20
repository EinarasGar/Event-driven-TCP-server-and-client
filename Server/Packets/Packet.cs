using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Server.Packets
{
    public class Packet
    {
        private byte[] _data;

        public virtual PacketType Type
        { get { return PacketType.Other; } }

        public virtual void Read(PacketReader r)
        {
            _data = r.ReadBytes((int)r.BaseStream.Length); // All of the packet data
        }

        public virtual void Write(PacketWriter w)
        {
            w.Write(_data); // All of the packet data
        }

        public enum PacketType
        {
            Hello,
            Info,
            Message,
            Failure,
            Other
        }

        internal static Packet Create(byte[] bytes)
        {
            using (PacketReader r = new PacketReader(bytes))
            {
                PacketType header = (PacketType)r.ReadInt16();

                Type[] types = typeof(Packet).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Packet))).ToArray(); // Get all class types that inherits Packet class

                Type Type = null;
                foreach (Type pType in types)
                {
                    PacketType t = (Activator.CreateInstance(pType) as Packet).Type;
                    if (t == header)
                    {
                        Type = pType;
                    }
                }

                Packet packet = (Packet)Activator.CreateInstance(Type);
                packet.Read(r);

                return packet;
            }
        }

        public override string ToString()
        {
            FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            StringBuilder s = new StringBuilder();
            s.Append("[" + Type + "] Packet Instance");
            foreach (FieldInfo f in fields)
                s.Append("\n\t" + f.Name + " => " + f.GetValue(this));
            s.Append("\n");
            return s.ToString();
        }
    }
}
