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
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");
            
            Program.Room.Push(() => Program.Room.Enter(this));
        }

        public override void OnDisConnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);
            if(Room != null) 
            {
                GameRoom room = Room;
                Room.Push(() => room.Leave(this));
                Room = null;
            }
            Console.WriteLine($"OnDisConnected : {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfByte)
        {
            //Console.WriteLine($"Transferred bytes : {numOfByte} ");
        }
    }
}
