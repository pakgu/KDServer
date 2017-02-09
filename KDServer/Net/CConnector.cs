using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KDServer.Net
{
    class CConnector
    {
        public delegate void ConnectedHandler(CUserToken token);
        public ConnectedHandler Callback_Connected { get; set; }

        Socket client;
        CNetworkService networkService;

        public CConnector(CNetworkService net_)
        {
            this.networkService = net_;
            this.Callback_Connected = null;
        }

        public void Connect(IPEndPoint endPoint_)
        {
            this.client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += On_ConnectCompleted;
            args.RemoteEndPoint = endPoint_;

            bool pending = this.client.ConnectAsync(args);
            if (!pending)
            {
                On_ConnectCompleted(null, args);
            }
        }

        public void On_ConnectCompleted(object sender, SocketAsyncEventArgs args_)
        {
            if (args_.SocketError == SocketError.Success)
            {
                CUserToken token = new CUserToken();

                this.networkService.On_CompletedConnect(this.client, token);

                if (this.Callback_Connected != null)
                {
                    this.Callback_Connected(token);
                }
            }
            else
            {
                Console.WriteLine(string.Format("Fail CConnector:On_ConnectCompleted. {0}", args_.SocketError));
            }
        }
    }
}
