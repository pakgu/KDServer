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
            // ex)
            CPacket packet = new CPacket(buffer.Value, this);
            EVENTID protocol = (EVENTID)packet.PopEventID();

            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine("protocol id " + protocol);

            switch (protocol)
            {
                case EVENTID.E_REQ:
                    {
                        string text = packet.Get_string();
                        Console.WriteLine(string.Format("text {0}", text));

                        CPacket response = CPacket.Create((short)EVENTID.E_ACK);
                        response.Put(text);
                        SendPacket(response);
                    }
                    break;
            }
        }

        public void SendPacket(CPacket _packet)
        {
            this.token.SendPacket(_packet);
        }

        void IPeer.OnRemoved()
        {
            Console.WriteLine("The client disconnected.");

//            Program.remove_user(this);
        }

        void IPeer.Disconnect()
        {
            this.token.socket.Disconnect(false);
        }

        void IPeer.ProcessPacket(CPacket _packet)
        {
        }
    }
}
