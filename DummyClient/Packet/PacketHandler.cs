using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{

    internal static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        S_Chat schatPacket = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;


     //   Console.WriteLine($"{schatPacket.playerId} : {schatPacket.chat} ");
        
    }       
}

 