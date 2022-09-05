using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{

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



	public enum PacketID 
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2, 
    }

    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");
            var packet = new PlayerInfoReq() { playerId = 1001,name = "asdfadsf" };
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 102, level = 3, duration = 1.5f });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 103, level = 1, duration = 3.3f });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 104, level = 5, duration = 4.3f });
            packet.skills.Add(new PlayerInfoReq.Skill() { id = 105, level = 4, duration = 6.3f });
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
