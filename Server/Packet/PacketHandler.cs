using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    public static void C_PlayerInfoReqHandler(PacketSession session , IPacket packet) 
    {
        C_PlayerInfoReq p = packet as C_PlayerInfoReq;

        Console.WriteLine($"Player ID  : {p.playerId} Name: {p.name}");
        foreach (var skill in p.skills)
        {
            Console.WriteLine($"skill : {skill.id} {skill.level} {skill.duration}");
        }
    }

}

