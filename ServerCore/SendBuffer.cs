using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{

    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(()=>null);
        public static int ChunkSize { get; set; } = 4096 * 100;
        public static ArraySegment<byte> Open(int reserveSize) 
        {
            if(CurrentBuffer.Value == null) 
            {
                CurrentBuffer.Value = new SendBuffer(ChunkSize);
            }    
            if(CurrentBuffer.Value.FreeSize  < reserveSize) 
            {
                CurrentBuffer.Value = new SendBuffer(ChunkSize);
            }
            return CurrentBuffer.Value.Open(reserveSize);
        }
        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer
    {

        byte[] _buffer;
        int _usedSize = 0;

        public SendBuffer(int chunkSize) 
        {
            _buffer = new byte[chunkSize];
        }
        public int FreeSize { get => _buffer.Length - _usedSize; }
        public ArraySegment<byte> Open(int _reserveSize) 
        {
            if (_reserveSize > FreeSize)
            {
                return new ArraySegment<byte>(null);
            }
            return new ArraySegment<byte>(_buffer, _usedSize, _reserveSize);
        }
        public ArraySegment<byte> Close(int usedSize) 
        {
          
            var segement = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;
            return segement;
        }
    }
}
