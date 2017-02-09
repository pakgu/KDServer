using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KDServer.Net;

namespace KDServer
{
    class CUser : IPeer
    {
        CUserToken token;

        public CUser(CUserToken token)
		{
			this.token = token;
			this.token.SetPeer(this);
		}

        void IPeer.OnPacket(Const<byte[]> buffer)
        {
            byte[] clone = new byte[Define.BUFFER_SIZE];
            Array.Copy(buffer.Value, clone, buffer.Value.Length);
            CPacket packet = new CPacket(clone, this);

            CServer.GetInstance().EnqueuePacket(packet, this);
        }

        void IPeer.OnRemoved()
        {
            CServer.GetInstance().RemoveUser(this);
        }

        void IPeer.Disconnect()
        {
            this.token.socket.Disconnect(false);
        }

        void IPeer.ProcessPacket(CPacket packet_)
        {
            EVENTID protocol = (EVENTID)packet_.PopEventID();

            switch (protocol)
            {
                case EVENTID.E_REQ:
                    {
                        string text = packet_.Get_string();
                        Console.WriteLine(string.Format("text {0}", text));

                        CPacket packet = CPacket.Create((short)EVENTID.E_ACK);
                        packet.Put(text);
                        CServer.GetInstance().BroadCast(packet);
                    }
                    break;
            }

        }

        public void SendPacket(CPacket packet_)
        {
            this.token.SendPacket(packet_);
        }

        public CUserToken GetToken()
        {
            return token;
        }
    }
}
