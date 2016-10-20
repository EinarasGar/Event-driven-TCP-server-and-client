using Server.Packets.Client;
using Server.Packets.Server;
using System;
using System.Net.Sockets;

namespace Server
{
    public class PacketManager
    {
        Networking Networking;
        public void Attach(Networking Networking)
        {
            this.Networking = Networking;
            Networking.HookPacket<HelloPacket>(HelloPacketRecieved);
            Networking.ClientConnected += Networking_ClientConnected;
        }

        private void Networking_ClientConnected(Socket client)
        {
            MessagePacket welcome = new MessagePacket();
            welcome.MessageType = MessagePacket.MessageTypes.Announcement;
            welcome.String = "Hello!";
            client.Send(welcome);
        }


        private void HelloPacketRecieved(Socket client, HelloPacket packet)
        {
            Console.WriteLine("Recieved HELLO packet from {0}", packet.Name);

            if (packet.Password == "Password")
            {
                InfoPacket info = new InfoPacket();
                info.ConnectedClients = Networking.ConnectedClients.Count;
                info.Message = "Welcome " + packet.Name + "!";
                client.Send(info);
            }
            else
            {
                FailurePacket failure = new FailurePacket();
                failure.ErrorId = 0;
                failure.ErrorMessage = "WRONG PASSWORD!";
                client.Send(failure);
                Networking.Kick(client);
            }
        }
        public void SendRequest() // Loops in while(true) loop
        {
            string request = Console.ReadLine();
            MessagePacket message = new MessagePacket();
            message.String = request;
            foreach (Socket Client in Networking.ConnectedClients)
            {
                Client.Send(message);
            }
        }
    }
}
