using ServerCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Server
{
    abstract class Packet
    {
        public ushort size;
        public ushort id;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);

    }
    class PlayterInfoReq : Packet
    {
        public long playerId;
        public string name;
        public List<SkillInfo> skills = new List<SkillInfo>();
        public struct SkillInfo
        {
            public int id;
            public short level;
            public float duration;
            public bool Write(Span<byte> s, ref ushort count)
            {
                bool success = true;

                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), id);
                count += sizeof(int);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), level);
                count += sizeof(short);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), duration);
                count += sizeof(float);
                return success;
            }

            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {

                id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);

                level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                count += sizeof(short);

                duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                count += sizeof(float);
            }

        }


        class PlayerInfoReq
        {
            public long playerId;
            public string name;

            public struct Skill
            {
                public int id;
                public short level;
                public float duration;

                public void Read(ReadOnlySpan<byte> s, ref ushort count)
                {
                    this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                    count += sizeof(int);
                    this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
                    count += sizeof(short);
                    this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
                    count += sizeof(float);
                }

                public bool Write(Span<byte> s, ref ushort count)
                {
                    bool success = true;
                    success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
                    count += sizeof(int);

                    success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
                    count += sizeof(short);

                    success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
                    count += sizeof(float);

                    return success;
                }

            }
            public List<Skill> skills = new List<Skill>();


            public void Read(ArraySegment<byte> segment)
            {
                ushort count = 0;

                ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
                count += sizeof(ushort);
                count += sizeof(ushort);
                this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
                count += sizeof(long);
                ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
                count += sizeof(ushort);
                this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
                count += nameLen;

                skills.Clear();
                ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
                count += sizeof(ushort);
                for (int i = 0; i < skillLen; ++i)
                {
                    Skill skill = new Skill();
                    skill.Read(s, ref count);
                    skills.Add(skill);
                }


            }

            public ArraySegment<byte> Write()
            {

                var segment = SendBufferHelper.Open(4096);
                ushort count = 0;
                bool success = true;

                Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
                count += sizeof(ushort);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
                count += sizeof(ushort);
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
                count += sizeof(long);


                var nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
                success &= BitConverter.TryWriteBytes(s.Slice(count, nameLen), nameLen);
                count += sizeof(ushort);
                count += nameLen;


                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
                count += sizeof(ushort);
                foreach (Skill skill in this.skills)
                {
                    success &= skill.Write(s, ref count);
                }

                success &= BitConverter.TryWriteBytes(s, count);
                if (success == false)
                {
                    return null;
                }
                return SendBufferHelper.Close(count);
            }
        }


        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
            // ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += sizeof(ushort);
            // ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += sizeof(ushort);

            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));

            count += sizeof(long);

            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
            count += nameLen;

            ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            skills.Clear();

            for (int i = 0; i < skillLen; ++i)
            {
                var skill = new SkillInfo();
                skill.Read(s, ref count);
                skills.Add(skill);
            }

        }

        public override ArraySegment<byte> Write()
        {

            var segment = SendBufferHelper.Open(4096);
            ushort count = 0;
            bool success = true;

            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);


            var nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(count, nameLen), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            // skill list 
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
            count += sizeof(ushort);

            foreach (SkillInfo skill in skills)
            {
                success &= skill.Write(s, ref count);
            }


            success &= BitConverter.TryWriteBytes(s, count);

            if (success == false)
            {
                return null;
            }
            return SendBufferHelper.Close(count);
        }
    }


    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }

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
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;
            
            switch((PacketID)id) 
            {
                case PacketID.PlayerInfoReq:
                    {
                        PlayterInfoReq p = new PlayterInfoReq();
                        p.Read(buffer);
                      
                        Console.WriteLine($"Player ID  : {p.playerId} Name: {p.name}");
                        foreach(var skill in p.skills) 
                        {
                            Console.WriteLine($"skill : {skill.id} {skill.level} {skill.duration}");
                        }
                    
                    }
                    break;
                case PacketID.PlayerInfoOk:
                    break;
            }

            Console.WriteLine($"RecvPacket Size : {size} ID : {id}");
        }

        public override void OnSend(int numOfByte)
        {
            Console.WriteLine($"Transferred bytes : {numOfByte} ");
        }
    }
}
