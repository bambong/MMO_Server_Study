using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;


namespace DummyClient
{

	class ServerSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");
            C_Chat packet = new C_Chat() { chat = "Connected!"};
            var segment = packet.Write();

            Send(segment);
        }

        public override void OnDisConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisConnected : {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }
        public override void OnSend(int numOfByte)
        {
           // Console.WriteLine($"Transferred bytes : {numOfByte}");
        }
    }
}
