using Server.Packets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


namespace Server
{
    public delegate void ClientConnected(Socket Client);
    public delegate void ClientDisonnected(Socket Client);
    public delegate void PacketHandler(Socket Client, Packet Packet);
    public delegate void GenericPacketHandler<T>(Socket Client, T Packet) where T : Packet;

    public static class ExtensionMethods
    {
        /// <summary>
        /// Extends Socket class, so I can send packet by calling Client.Send(Packet packet);
        /// </summary>
        /// <param name="Client">Socket client, that we want to send packet to.</param>
        /// <param name="Packet">Packet type with fields</param>
        public static void Send(this Socket Client, Packet Packet)
        {
            using (PacketWriter w = new PacketWriter())
            {
                w.Write((ushort)Packet.Type);
                Packet.Write(w);

                byte[] data = w.GetBytes();
                Client.Send(data, 0, data.Length, SocketFlags.None);
            }
        }
    }

    public class Networking
    {
        public event PacketHandler PacketRecieved;
        public event ClientConnected ClientConnected;
        public event ClientConnected ClientDisonnected;

        private readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int BUFFER_SIZE = 2048;
        private const int PORT = 100;
        private readonly byte[] buffer = new byte[BUFFER_SIZE];

        public readonly List<Socket> ConnectedClients = new List<Socket>();
        private Dictionary<object, Type> PacketHooks;
        private PacketManager Manager;

        public Networking()
        {
            PacketHooks = new Dictionary<object, Type>();
            SetupServer();
            Manager = new PacketManager();
            ClientConnected += Networking_ClientConnected;
            ClientDisonnected += Networking_ClientDisonnected;
            Manager.Attach(this);
            RequestLoop();
        }

        private void Networking_ClientConnected(Socket Client)
        {
            ConnectedClients.Add(Client);
            Log("Client connected!    IP: " + Client.LocalEndPoint, ConsoleColor.Green);
            Console.Title = "Server - connected clients: " + ConnectedClients.Count;
        }

        private void Networking_ClientDisonnected(Socket client)
        {
            ConnectedClients.Remove(client);
            Log("Client disconnected. IP: " + client.LocalEndPoint, ConsoleColor.DarkYellow);
            Console.Title = "Server - connected clients: " + ConnectedClients.Count;
        }

        void RequestLoop()
        {
            while (true)
            {
                Manager.SendRequest();
            }
        }

        private void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(_ClientConnected, null);
            Console.Clear();
            Log("Server setup complete", ConsoleColor.White);

        }

        public void CloseAllSockets()
        {
            foreach (Socket socket in ConnectedClients)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            serverSocket.Close();
        }

        private void _ClientConnected(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            if (ClientConnected != null) ClientConnected(socket);

            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, _PacketRecieved, socket);
            serverSocket.BeginAccept(_ClientConnected, null);
        }

        private void _PacketRecieved(IAsyncResult AR)
        {
            Socket Client = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = Client.EndReceive(AR);
            }
            catch (Exception ex)
            {
                if (ex is ObjectDisposedException)
                {
                    return;
                }
                Kick(Client);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);

            Client.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, _PacketRecieved, Client);
            ParsePacket(recBuf, Client);
        }

        public void Kick(Socket client)
        {
            ClientDisonnected(client);
            client.Close();
            ConnectedClients.Remove(client);
        }

        private void ParsePacket(byte[] bytes, Socket Client)
        {
            PacketReader pr = new PacketReader(bytes);
            Packet packet = Packet.Create(bytes);
            FireServerPacket(Client, packet);
        }

        public void FireServerPacket(Socket client, Packet packet)
        {
            if (PacketRecieved != null) PacketRecieved(client, packet); // General packet recieved event

            foreach (var pair in PacketHooks) // calls all hooked packets
                if (pair.Value == packet.GetType())
                    (pair.Key as Delegate).Method.Invoke((pair.Key as Delegate).Target, new object[2] { client, Convert.ChangeType(packet, pair.Value) });
        }

        public void HookPacket<T>(GenericPacketHandler<T> callback) where T : Packet
        {
            if (!PacketHooks.ContainsKey(callback))
                PacketHooks.Add(callback, typeof(T));
            else
                throw new InvalidOperationException("Callback already bound");
        }

        private void Log(string text, ConsoleColor color1)
        {
            Console.ForegroundColor = color1;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
