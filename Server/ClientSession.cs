using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Server
{

	class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            //Packet packet = new Packet() { size = 4, id = 10 };

            //var openSegement = SendBufferHelper.Open(4096);
            //byte[] buffer = BitConverter.GetBytes(packet.size);
            //byte[] buffer2 = BitConverter.GetBytes(packet.id);
            //Array.Copy(buffer, 0, openSegement.Array, openSegement.Offset, buffer.Length);
            //Array.Copy(buffer2, 0, openSegement.Array, openSegement.Offset + buffer.Length, buffer2.Length);
            //var sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);

            //Send(sendBuff);


            Thread.Sleep(1000);
            DisConnect();
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
            Console.WriteLine($"Transferred bytes : {numOfByte} ");
        }
    }
}
