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
    class CNetworkService
    {
        int connectedCount;
        CListener listener;
        SocketAsyncEventArgsPool recvEventArgsPool;
        SocketAsyncEventArgsPool sendEventArgsPool;
        BufferManager bufferManager;

        public delegate void SessionHandler(CUserToken token);
        public SessionHandler callback_CreatedSession { get; set; }


        public CNetworkService()
        {
            this.connectedCount = 0;
            this.callback_CreatedSession = null;
        }

        public void Init()
        {
            this.bufferManager = new BufferManager(Define.MAX_CONNECT * Define.BUFFER_SIZE * 2, Define.BUFFER_SIZE);
            this.recvEventArgsPool = new SocketAsyncEventArgsPool(Define.MAX_CONNECT);
            this.sendEventArgsPool = new SocketAsyncEventArgsPool(Define.MAX_CONNECT);

            this.bufferManager.InitBuffer();

            SocketAsyncEventArgs arg;

            for (int i = 0; i < Define.MAX_CONNECT; i++)
            {
                CUserToken token = new CUserToken();

                {
                    arg = new SocketAsyncEventArgs();
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(CompletedRecv);
                    arg.UserToken = token;

                    this.bufferManager.SetBuffer(arg);
                    this.recvEventArgsPool.Push(arg);
                }

               {
                    arg = new SocketAsyncEventArgs();
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(CompletedSend);
                    arg.UserToken = token;

                    this.bufferManager.SetBuffer(arg);
                    this.sendEventArgsPool.Push(arg);
                }
            }
        }

        public void Listen(string host, int port, int backlog)
        {
            this.listener = new CListener();
            this.listener.callback_OnNewclient += On_NewClient;
            this.listener.start(host, port, backlog);
        }


        public void On_CompletedConnect(Socket socket, CUserToken token)
        {
            // SocketAsyncEventArgsPool에서 빼오지 않고 그때 그때 할당해서 사용한다.
            // 풀은 서버에서 클라이언트와의 통신용으로만 쓰려고 만든것이기 때문이다.
            // 클라이언트 입장에서 서버와 통신을 할 때는 접속한 서버당 두개의 EventArgs만 있으면 되기 때문에 그냥 new해서 쓴다.
            // 서버간 연결에서도 마찬가지이다.
            // 풀링처리를 하려면 c->s로 가는 별도의 풀을 만들어서 써야 한다.
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(CompletedRecv);
            recvArgs.UserToken = token;
            recvArgs.SetBuffer(new byte[Define.BUFFER_SIZE], 0, Define.BUFFER_SIZE);

            SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(CompletedSend);
            sendArgs.UserToken = token;
            sendArgs.SetBuffer(new byte[Define.BUFFER_SIZE], 0, Define.BUFFER_SIZE);

            BeginRecv(socket, recvArgs, sendArgs);
        }

        void On_NewClient(Socket client_socket, object token)
        {
            Interlocked.Increment(ref this.connectedCount);

            Console.WriteLine(string.Format("[{0}] A client connected. handle {1},  count {2}",
                Thread.CurrentThread.ManagedThreadId, client_socket.Handle,
                this.connectedCount));

            // 플에서 하나 꺼내와 사용한다.
            SocketAsyncEventArgs recvArgs = this.recvEventArgsPool.Pop();
            SocketAsyncEventArgs sendArgs = this.sendEventArgsPool.Pop();

            CUserToken user_token = null;
            if (this.callback_CreatedSession != null)
            {
                user_token = recvArgs.UserToken as CUserToken;
                this.callback_CreatedSession(user_token);
            }

            BeginRecv(client_socket, recvArgs, sendArgs);
        }

        void BeginRecv(Socket socket, SocketAsyncEventArgs recvArgs, SocketAsyncEventArgs sendArgs)
        {
            CUserToken token = recvArgs.UserToken as CUserToken;
            token.SetArgs(recvArgs, sendArgs);
            token.socket = socket;

            bool pending = socket.ReceiveAsync(recvArgs);
            if (!pending)
            {
                ProcessRecv(recvArgs);
            }
        }

        void CompletedRecv(object sender, SocketAsyncEventArgs args)
        {
            if (args.LastOperation == SocketAsyncOperation.Receive)
            {
                ProcessRecv(args);
                return;
            }

            throw new ArgumentException("The last operation completed on the socket was not a receive.");
        }

        void ProcessRecv(SocketAsyncEventArgs args)
        {
            CUserToken token = args.UserToken as CUserToken;
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                token.on_Recv(args.Buffer, args.Offset, args.BytesTransferred);

                bool pending = token.socket.ReceiveAsync(args);
                if (!pending)
                {
                    ProcessRecv(args);
                }
            }
            else
            {
                Console.WriteLine(string.Format("error {0},  transferred {1}", args.SocketError, args.BytesTransferred));
                CloseSocket(token);
            }
        }

        void CompletedSend(object sender, SocketAsyncEventArgs args)
        {
            CUserToken token = args.UserToken as CUserToken;
            token.ProcessSend(args);
        }

        public void CloseSocket(CUserToken token)
        {
            token.on_Removed();

            // Free the SocketAsyncEventArg so they can be reused by another client
            // 버퍼는 반환할 필요가 없다. SocketAsyncEventArg가 버퍼를 물고 있기 때문에
            // 이것을 재사용 할 때 물고 있는 버퍼를 그대로 사용하면 되기 때문이다.
            if (this.recvEventArgsPool != null)
            {
                this.recvEventArgsPool.Push(token.recvArgs);
            }

            if (this.sendEventArgsPool != null)
            {
                this.sendEventArgsPool.Push(token.sendArgs);
            }
        }
    }
}
