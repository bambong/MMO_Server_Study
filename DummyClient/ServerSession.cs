using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;


namespace DummyClient
{

	class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");
            var packet = new C_PlayerInfoReq() { playerId = 1001,name = "asdfadsf" , testByte = 6,testSByte = -5 };
			var skill = new C_PlayerInfoReq.Skill() { id = 101, level = 6, duration = 1.5f };
			skill.attributes.Add(new C_PlayerInfoReq.Skill.Attribute() { att = 100 });
			packet.skills.Add(skill);
			packet.skills.Add(new C_PlayerInfoReq.Skill() { id = 102, level = 3, duration = 1.5f});
            packet.skills.Add(new C_PlayerInfoReq.Skill() { id = 103, level = 1, duration = 3.3f });
            packet.skills.Add(new C_PlayerInfoReq.Skill() { id = 104, level = 5, duration = 4.3f });
            packet.skills.Add(new C_PlayerInfoReq.Skill() { id = 105, level = 4, duration = 6.3f });
            //for (int i = 0; i < 5; ++i)

            var s = packet.Write();
        
            if (s != null)
            {
                Send(s);
            }
           
        }

        public override void OnDisConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisConnected : {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {

            var recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);

            Console.WriteLine($"[From Server] {recvData}  size : {buffer.Count}");
            return buffer.Count;
        }

        public override void OnSend(int numOfByte)
        {
            Console.WriteLine($"Transferred bytes : {numOfByte}");
        }
    }
}
