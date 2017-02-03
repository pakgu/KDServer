using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace KDServer.Net
{
    class CListener
    {
        SocketAsyncEventArgs acceptArgs;
        Socket socket;

        AutoResetEvent evFlag;

        public delegate void NewclientHandler(Socket socket, object token);
        public NewclientHandler callback_OnNewclient;

        public CListener()
        {
            this.callback_OnNewclient = null;
        }

        public void start(string host, int port, int backlog)
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress address;
            if (host == "0.0.0.0")
            {
                address = IPAddress.Any;
            }
            else
            {
                address = IPAddress.Parse(host);
            }
            IPEndPoint endpoint = new IPEndPoint(address, port);

            try
            {
                socket.Bind(endpoint);
                socket.Listen(backlog);

                this.acceptArgs = new SocketAsyncEventArgs();
                this.acceptArgs.Completed += new EventHandler<SocketAsyncEventArgs>(On_CompletedAccept);

                Thread listen_thread = new Thread(_Listen);
                listen_thread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        void _Listen()
        {
            this.evFlag = new AutoResetEvent(false);

            while (true)
            {
                this.acceptArgs.AcceptSocket = null;

                bool pending = true;
                try
                {
                    pending = socket.AcceptAsync(this.acceptArgs);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                if (!pending)
                {
                    On_CompletedAccept(null, this.acceptArgs);
                }

                this.evFlag.WaitOne();
            }
        }

        void On_CompletedAccept(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Socket socket = args.AcceptSocket;

                this.evFlag.Set();

                if (this.callback_OnNewclient != null)
                {
                    this.callback_OnNewclient(socket, args.UserToken);
                }

                return;
            }
            else
            {
                Console.WriteLine("Accept Failed.");
            }

            this.evFlag.Set();
        }
    }
}
