using System;
using System.Net;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Client";
            Networking net = new Networking(IPAddress.Loopback.ToString(), 100);
        }
    }
}
