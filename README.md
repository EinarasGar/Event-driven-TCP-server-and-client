# Event driven TCP server and client.

This repository contains TCP server and client core, which allows you to hook incoming packets, read them and respond to them instatnly.


# How to use.
```
Server events:
  PacketRecieved 
  ClientConnected
  ClientDisonnected
  
Client events:
  Connected
  ServerPacketRecieved
  
Hook packets by using:
  HookPacket<PackeType>(CallBackFunction);
  
Create packets by making new instance of the packet:
  MessagePacket welcome = new MessagePacket();
  welcome.MessageType = MessagePacket.MessageTypes.Announcement;
  welcome.String = "Hello!";

Send packet by calling:
  Socket.send(Packet);
  
Expand packet list by adding new class in client or server folder making sure to inherit Packet
class and overriding PacketType, Read, Write 
  Example of expanding packet list:    
    class ExamplePacket : Packet
    {
        public int ExampleInt;
        public string ExampleString;

        public override PacketType Type
        { get { return PacketType.Example; } }

        public override void Read(PacketReader r)
        {
            ExampleInt = r.ReadInt32();
            ExampleString = r.ReadString();
        }

        public override void Write(PacketWriter w)
        {
            w.Write(ExampleInt);
            w.Write(ExampleString);
        }
    }
    
```


Example is included in PacketManager class in both projects.
