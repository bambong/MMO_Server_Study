using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;
            int packetCount = 0;
            while (true)
            {

                // 최소한 헤더는 파싱가능 한지 확인
                if (buffer.Count < HeaderSize)
                {
                    //Console.WriteLine($"헤더를 파싱할수 없습니다. buffer 사이즈 : {buffer.Count}");
                    break;
                }
                // 패킷이 완전체로 도착했는지 확인

                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                {
                    //Console.WriteLine($"패킷 데이터가 모자랍니다. buffer 사이즈 : {buffer.Count}");
                    break;
                }
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));
                packetCount++;
                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }
            if(packetCount > 1) 
            {
                Console.WriteLine($"패킷 모아보내기 {packetCount}");
            }
            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);

    }
    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();

        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        RecvBuffer _recvBuffer = new RecvBuffer(65535);

        object _lock = new object();


        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnDisConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<Byte> buffer);
        public abstract void OnSend(int numOfByte);

        void Clear() 
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        
        }

        public void Start(Socket socket)
        {
            _socket = socket;

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            _recvArgs.SetBuffer(new byte[1024], 0, 1024);

            RegisterRecv();
        }

        public void DisConnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            {
                return;
            }
            OnDisConnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();
        }
        public void Send(List<ArraySegment<byte>> sendBuffList)
        {
            if(sendBuffList.Count == 0) 
            {
                return;
            }
            lock (_lock)
            {
                foreach(var sendbuff in sendBuffList) 
                {
                    _sendQueue.Enqueue(sendbuff);
                }
                if (_pendingList.Count == 0)
                {
                    RegisterSend();
                }
            }
        }
        public void Send(ArraySegment<byte> sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                {
                    RegisterSend();
                }
            }
        }
        #region 네트워크 통신
        void RegisterSend()
        {
            if (_disconnected == 1)
            {
                return;
            }

            while (_sendQueue.Count > 0) 
            {

                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
            
            }
            _sendArgs.BufferList =  _pendingList;


            try 
            {
                bool pending = _socket.SendAsync(_sendArgs);
                if (pending == false)
                {
                    OnSendCompleted(null, _sendArgs);
                }

            }
            catch (Exception e) 
            {
                Console.WriteLine($"RegisterSend Failed {e}");
            }

    
        }
        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {

                        _sendArgs.BufferList = null;
                        _pendingList.Clear();
                        OnSend(_sendArgs.BytesTransferred);
                        
                        if (_sendQueue.Count() != 0)
                        {
                            RegisterSend();
                        }
                     
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"On Send Failed !! {e}");
                    }

                }
                else
                {
                    DisConnect();
                }
            }
        }
        void RegisterRecv()
        {
            if (_disconnected == 1)
            {
                return;
            }

            try 
            {
                _recvBuffer.Clean();
                var segement = _recvBuffer.WriteSegement;
                _recvArgs.SetBuffer(segement.Array, segement.Offset, segement.Count);

                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (pending == false)
                {
                    OnRecvCompleted(null, _recvArgs);
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine($"RegisterRecv Failed {e}");
            }
           
        }
       
        void OnRecvCompleted(object sender,SocketAsyncEventArgs args ) 
        {
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success) 
            {
                try 
                {
                    //Write 커서 이동
                    if( _recvBuffer.OnWrite(args.BytesTransferred) == false) 
                    {
                        DisConnect();
                        return;
                    }

                    int processLen =  OnRecv(_recvBuffer.ReadSegment);
                    if(processLen <= 0 || _recvBuffer.DataSize < processLen)
                    {
                        DisConnect();
                        return;
                    }
                    // Read 커서 이동

                    if(_recvBuffer.OnRead(processLen) == false)
                    {
                        DisConnect();
                        return;
                    }
                    RegisterRecv();
                }
                catch(Exception e) 
                {
                    Console.WriteLine($"On Recv Failed !! {e}");
                }
            }
            else 
            {
                // ToDO
                DisConnect();
            }
        
        }
        #endregion
    }
}
