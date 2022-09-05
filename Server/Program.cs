using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{ 
    class Program
    {

        static Listener listener = new Listener();

        static void Main(string[] args)
        {
            var host = Dns.GetHostName();
            var ipHost = Dns.GetHostEntry(host);
            var ipAddress = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777);


            listener.Init(endPoint, () => new ClientSession());
            Console.WriteLine("Listening....");

            while (true)
            {

            }

        }
    }
}
