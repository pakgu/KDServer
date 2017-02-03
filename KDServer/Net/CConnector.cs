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

        CNetworkService network_service;

    }
}
