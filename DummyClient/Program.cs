using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server;
using ServerCore;

namespace DummyClient
{

    class Program
    {
        static void Main(string[] args)
        {
            var host = Dns.GetHostName();
            var ipHost = Dns.GetHostEntry(host);
            var ipAddress = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777);
            Connector connector = new Connector();
            connector.Connect(endPoint, () => SessionManager.Instance.Generate(),500);
            while (true) 
            {
                try 
                {
                    SessionManager.Instance.SendForEach();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(250);            }
        }
    }
}
