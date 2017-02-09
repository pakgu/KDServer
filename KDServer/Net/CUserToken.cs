using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace KDServer.Net
{
    public class CUserToken
    {
        public Socket socket { get; set; }

        public SocketAsyncEventArgs recvArgs { get; private set; }
        public SocketAsyncEventArgs sendArgs { get; private set; }

        CPacketResolver packetResolver;
        IPeer peer;

        ConcurrentQueue<CPacket> queueSendPacket;

        static int sentCount = 0;


        public CUserToken()
        {
            this.packetResolver = new CPacketResolver();
            this.peer = null;

            this.queueSendPacket = new ConcurrentQueue<CPacket>();
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
            CPacket dummy;
            while (this.queueSendPacket.TryDequeue(out dummy)) ;

            if (this.peer != null)
            {
                this.peer.OnRemoved();
            }
        }

        public void SendPacket(CPacket packet_)
        {
            CPacket packet = new CPacket();
            packet_.Copy_to(packet);
            
            if (this.queueSendPacket.IsEmpty)
            {
                this.queueSendPacket.Enqueue(packet);
                BeginSend();
                return;
            }

            this.queueSendPacket.Enqueue(packet);
        }

        void BeginSend()
        {
            // 전송이 아직 완료된 상태가 아니므로 데이터만 가져오고 큐에서 제거하진 않는다.
            CPacket packet = null;
            if( false == this.queueSendPacket.TryPeek(out packet) )
                return;

            packet.RecordDataSize();

            this.sendArgs.SetBuffer(this.sendArgs.Offset, packet.position);
            Array.Copy(packet.buffer, 0, this.sendArgs.Buffer, this.sendArgs.Offset, packet.position);

            bool pending = this.socket.SendAsync(this.sendArgs);
            if (!pending)
            {
                ProcessSend(this.sendArgs);
            }
        }

        public void ProcessSend(SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred <= 0 || args.SocketError != SocketError.Success)
            {
                Console.WriteLine(string.Format("Failed to send. error {0}, transferred {1}", args.SocketError, args.BytesTransferred));
                return;
            }

            if (this.queueSendPacket.IsEmpty)
            {
                return;
            }

            System.Threading.Interlocked.Increment(ref sentCount);

            CPacket dummy;
            this.queueSendPacket.TryDequeue(out dummy);

            if (this.queueSendPacket.IsEmpty)
            {
                BeginSend();
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
