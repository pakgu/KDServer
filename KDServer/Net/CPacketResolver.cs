using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDServer.Net
{
    class CPacketResolver
    {
        public delegate void callback_CompletedMessage(Const<byte[]> buffer);

        int packetSize;
        byte[] buffer = new byte[Define.BUFFER_SIZE];

        int nowPos;
        int destPos;
        int remainSize;

        public CPacketResolver()
        {
            this.packetSize = 0;
            this.nowPos = 0;
            this.destPos = 0;
            this.remainSize = 0;
        }

        bool Read(byte[] buffer, ref int _pos, int offset, int transffered)
        {
            if (this.nowPos >= offset + transffered)
            {
                return false;
            }

            int needSize = this.destPos - this.nowPos;

            if (this.remainSize < needSize)
            {
                needSize = this.remainSize;
            }

            Array.Copy(buffer, _pos, this.buffer, this.nowPos, needSize);

            _pos += needSize;

            this.nowPos += needSize;
            this.remainSize -= needSize;

            if (this.nowPos < this.destPos)
            {
                return false;
            }

            return true;
        }

        public void on_Recv(byte[] buffer, int offset, int transffered, callback_CompletedMessage callback)
        {
            this.remainSize = transffered;

            int srcPos = offset;

            while (this.remainSize > 0)
            {
                bool completed = false;

                if (this.nowPos < Define.HEADER_SIZE)
                {
                    this.destPos = Define.HEADER_SIZE;

                    completed = Read(buffer, ref srcPos, offset, transffered);
                    if (!completed)
                    {
                        return;
                    }

                    this.packetSize = GetPacketSize();

                    this.destPos = this.packetSize + Define.HEADER_SIZE;
                }

                completed = Read(buffer, ref srcPos, offset, transffered);

                if (completed)
                {
                    callback(new Const<byte[]>(this.buffer));

                    ClearBuffer();
                }
            }
        }

        int GetPacketSize()
        {
            Type type = Define.HEADER_SIZE.GetType();
            if (type.Equals(typeof(Int16)))
            {
                return BitConverter.ToInt16(this.buffer, 0);
            }

            return BitConverter.ToInt32(this.buffer, 0);
        }

        void ClearBuffer()
        {
            Array.Clear(this.buffer, 0, this.buffer.Length);

            this.packetSize = 0;
            this.nowPos = 0;
            this.destPos = 0;
        }
    }
}
