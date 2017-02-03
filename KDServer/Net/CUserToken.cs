using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace KDServer.Net
{
    class CUserToken
    {
        public Socket socket { get; set; }

        public SocketAsyncEventArgs recvArgs { get; private set; }
        public SocketAsyncEventArgs sendArgs { get; private set; }

        CPacketResolver packetResolver;
        IPeer peer;

        Queue<CPacket> queueSendPacket;
        private object cs_queueSendPacket;

        static int sentCount = 0;


        public CUserToken()
        {
            this.packetResolver = new CPacketResolver();
            this.peer = null;

            this.queueSendPacket = new Queue<CPacket>();
            this.cs_queueSendPacket = new object();
        }

        public void SetPeer(IPeer peer)
        {
            this.peer = peer;
        }

        public void SetArgs(SocketAsyncEventArgs _recvArgs, SocketAsyncEventArgs _sendArgs)
        {
            this.recvArgs = _recvArgs;
            this.sendArgs = _sendArgs;
        }

        public void on_Recv(byte[] buffer, int offset, int transfered)
        {
            this.packetResolver.on_Recv(buffer, offset, transfered, callback_OnPacket);
        }

        void callback_OnPacket(Const<byte[]> buffer)
        {
            if (this.peer != null)
            {
                this.peer.OnPacket(buffer);
            }
        }

        public void on_Removed()
        {
            this.queueSendPacket.Clear();

            if (this.peer != null)
            {
                this.peer.OnRemoved();
            }
        }

        public void SendPacket(CPacket _packet)
        {
            CPacket packet = new CPacket();
            _packet.Copy_to(packet);

            lock (this.cs_queueSendPacket)
            {
                // 큐가 비어 있다면 큐에 추가하고 바로 비동기 전송 매소드를 호출한다.
                if (this.queueSendPacket.Count <= 0)
                {
                    this.queueSendPacket.Enqueue(packet);
                    BeginSend();
                    return;
                }

                this.queueSendPacket.Enqueue(packet);
            }
        }

        void BeginSend()
        {
            lock (this.cs_queueSendPacket)
            {
                // 전송이 아직 완료된 상태가 아니므로 데이터만 가져오고 큐에서 제거하진 않는다.
                CPacket packet = this.queueSendPacket.Peek();

                // 헤더에 패킷 사이즈를 기록한다.
                packet.RecordSize();

                // 이번에 보낼 패킷 사이즈 만큼 버퍼 크기를 설정하고
                this.sendArgs.SetBuffer(this.sendArgs.Offset, packet.position);
                // 패킷 내용을 SocketAsyncEventArgs버퍼에 복사한다.
                Array.Copy(packet.buffer, 0, this.sendArgs.Buffer, this.sendArgs.Offset, packet.position);

                // 비동기 전송 시작.
                bool pending = this.socket.SendAsync(this.sendArgs);
                if (!pending)
                {
                    ProcessSend(this.sendArgs);
                }
            }
        }

        public void ProcessSend(SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred <= 0 || args.SocketError != SocketError.Success)
            {
                Console.WriteLine(string.Format("Failed to send. error {0}, transferred {1}", args.SocketError, args.BytesTransferred));
                return;
            }

            lock (this.cs_queueSendPacket)
            {
                if (this.queueSendPacket.Count <= 0)
                {
                    return;
                }

                System.Threading.Interlocked.Increment(ref sentCount);

                Console.WriteLine(string.Format("processSend : {0}, transferred {1}, sent count {2}", args.SocketError, args.BytesTransferred, sentCount));

                this.queueSendPacket.Dequeue();

                // 아직 전송하지 않은 대기중인 패킷이 있다면 다시한번 전송을 요청한다.
                if (this.queueSendPacket.Count > 0)
                {
                    BeginSend();
                }
            }
        }

        public void Disconnect()
        {
            try
            {
                this.socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception) { }

            this.socket.Close();
        }

        public void KeepAlive()
        {
            System.Threading.Timer t = new System.Threading.Timer((object e) =>
            {
                CPacket packet = CPacket.Create(0);
                packet.Put(0);
                SendPacket(packet);
            }, null, 0, 3000);
        }
    }
}
