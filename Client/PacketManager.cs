using System;
using Client.Packets.Client;
using Client.Packets.Server;

namespace Client
{
    class PacketManager
    {
        Networking Networking;
        public void Attach(Networking Networking)
        {
            this.Networking = Networking;
            Networking.HookPacket<MessagePacket>(Message);
            Networking.HookPacket<InfoPacket>(Info);
            Networking.HookPacket<FailurePacket>(Failure);
        }

        private void Failure(FailurePacket packet)
        {
            Console.WriteLine("FAILURE: ({0}) {1}", packet.ErrorId, packet.ErrorMessage);
        }

        private void Info(InfoPacket packet)
        {
            Console.WriteLine(packet.Message + " Connected clients: " + packet.ConnectedClients);
        }

        private void Message(MessagePacket packet)
        {
            Console.WriteLine(packet.MessageType.ToString() + ": " + packet.Message);
        }

        public void SendRequest() // Loops in while(true) loop
        {
            string request = Console.ReadLine();

            HelloPacket Hello = new HelloPacket();
            Hello.Name = Environment.UserName;
            Hello.Password = "Password";

            Networking.Send(Hello);
        }
    }
}
