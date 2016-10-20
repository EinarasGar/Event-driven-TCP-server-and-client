using System;
using System.Net.Sockets;
using System.Collections.Generic;
using Client.Packets;

namespace Client
{
    public delegate void PacketHandler(Packet Packet);
    public delegate void GenericPacketHandler<T>(T Packet) where T : Packet;
    public delegate void Connected(Socket Client);

    public class Networking
    {
        public event Connected Connected;
        public event PacketHandler ServerPacketRecieved;
        private Dictionary<object, Type> PacketHooks;

        private const int BUFFER_SIZE = 2048;
        private byte[] buffer = new byte[BUFFER_SIZE];
        private Socket ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private PacketManager Manager;

        public Networking(string Ip, int Port)
        {
            PacketHooks = new Dictionary<object, Type>();

            Manager = new PacketManager();
            Manager.Attach(this);

            ConnectToServer(Ip, Port);
            RequestLoop();
            Exit();
        }

        private void ConnectToServer(string Ip, int PORT)
        {
            int attempts = 0;

            while (!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    Log("Connection attempt " + attempts, ConsoleColor.Red, ConsoleColor.Gray);
                    ClientSocket.Connect(Ip, PORT);
                }
                catch (SocketException)
                {
                    Console.Clear();
                }
            }

            Console.Clear();
            Log(string.Format("Connected to server on {0} and port {1}\n", Ip, PORT), ConsoleColor.Green, ConsoleColor.Gray);
            if (Connected != null) Connected(ClientSocket);
        }

        private void RequestLoop()
        {
            ClientSocket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, PacketRecieved, ClientSocket); // starts a loop of looking for new packets

            while (true)
            {
                Manager.SendRequest();
            }
        }

        private void PacketRecieved(IAsyncResult ar) // Function that is being called whenever client recieves data from server
        {
            Socket Client = (Socket)ar.AsyncState;
            int received;
            try
            {
                received = Client.EndReceive(ar);
            }
            catch (SocketException)
            {
                Log("Client forcefully disconnected", ConsoleColor.Red, ConsoleColor.Gray);
                Client.Close();
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);

            if (received == 0)
            {
                Log("Lost Connection To The Server", ConsoleColor.Red, ConsoleColor.Gray);
                return;

            }

            PacketReader pr = new PacketReader(recBuf);
            Packet packet = Packet.Create(recBuf);
            FirePackedRecieved(Client, packet);

            Client.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, PacketRecieved, Client); // Read incoming packet again
        }

        private void FirePackedRecieved(Socket client, Packet packet)
        {
            if (ServerPacketRecieved != null) ServerPacketRecieved(packet); // general packet recieved event

            foreach (var pair in PacketHooks) //Hook Packets
                if (pair.Value == packet.GetType())
                    (pair.Key as Delegate).Method.Invoke((pair.Key as Delegate).Target, new object[1] { Convert.ChangeType(packet, pair.Value) });
        }

        public void Exit()
        {
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            Console.Read();
            Environment.Exit(0);
        }

        public void Send(Packet packet)
        {
            using (PacketWriter w = new PacketWriter())
            {
                w.Write((ushort)packet.Type);
                packet.Write(w);

                byte[] data = w.GetBytes();
                ClientSocket.Send(data, 0, data.Length, SocketFlags.None);
            }
        }

        public void HookPacket<T>(GenericPacketHandler<T> callback) where T : Packet
        {
            if (!PacketHooks.ContainsKey(callback))
                PacketHooks.Add(callback, typeof(T));
            else
                throw new InvalidOperationException("Callback already bound");
        }
        private void Log(string text, ConsoleColor color1, ConsoleColor color2)
        {
            Console.ForegroundColor = color1;
            Console.WriteLine(text);
            Console.ForegroundColor = color2;
        }
    }
}
