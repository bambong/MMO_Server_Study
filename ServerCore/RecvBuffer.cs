using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{

    
    class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;

        public RecvBuffer(int bufferSize) 
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize { get => _writePos - _readPos; }
        public int FreeSize { get => _buffer.Count - _writePos; }
        public ArraySegment<byte> ReadSegment 
        {
            get => new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize);
        }
        public ArraySegment<byte> WriteSegement
        {
            get => new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize);
        }
        public void Clean() 
        {
            int dataSize = DataSize;
            if(dataSize == 0) 
            {
                // 남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
                _readPos = 0;
                _writePos = 0;

            }
            else 
            {

                //남은 데이터가 있으면 시작 위치로 복사

                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        
        }

        public bool OnRead(int numOfByte)
        {
            if(numOfByte > DataSize)
            {
                return false;
            }
            _readPos += numOfByte;
            return true;
        }

        public bool OnWrite(int numOfByte)
        {
            if (numOfByte > FreeSize)
            {
                return false;
            }
            _writePos += numOfByte;
            return true;
        }
    } 


}
