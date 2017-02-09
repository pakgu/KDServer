using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using KDServer.Net;

namespace KDServer
{
    class CServer
    {
        Thread logicThread;
        AutoResetEvent evFlag;

        ConcurrentQueue<CPacket> QueueLogicPacket;

        CUserManager userManager;

        private static CServer instance = null;
        public static CServer GetInstance()
        {
            if (instance == null)
                instance = new CServer();

            return instance;
        }

        public CServer()
		{
            this.evFlag = new AutoResetEvent(false);
            this.QueueLogicPacket = new ConcurrentQueue<CPacket>();

            this.logicThread = new Thread(LogicPacketLoop);
            this.logicThread.Start();

            this.userManager = new CUserManager();
		}

        public void Init()
        {
            CPacketBufferManager.Init(1000);

            CNetworkService service = new CNetworkService();

            service.callback_CreatedSession += On_CreatedSession;

            service.Init();
            service.Listen("0.0.0.0", 9300, 100);
        }

        void On_CreatedSession(CUserToken token)
        {
            CUser user = new CUser(token);
            userManager.AddUser(user);

            Console.WriteLine("Disconnected Client Socket: {0},  CCU: {1}", user.GetToken().socket.Handle, GetCCU());
        }

        public void RemoveUser(CUser user)
        {
            userManager.RemoveUser(user);

            Console.WriteLine("Disconnected Client Socket: {0},  CCU: {1}", user.GetToken().socket.Handle, GetCCU());
        }

        public int GetCCU()
        {
            return this.userManager.userlist.Count();
        }

        public void EnqueuePacket(CPacket packet, CUser user)
        {
            QueueLogicPacket.Enqueue(packet);
            evFlag.Set();
        }

        void ProcessPacket(CPacket packet_)
        {
            packet_.owner.ProcessPacket(packet_);
        }

        public void BroadCast(CPacket packet_)
        {
            foreach ( CUser user in this.userManager.userlist )
            {
                if( user != null )
                {
                    user.SendPacket(packet_);
                }
            }
        }




        void LogicPacketLoop()
        {
            while (true)
            {
                CPacket packet = null;
                if (false == this.QueueLogicPacket.IsEmpty)
                {
                    this.QueueLogicPacket.TryDequeue(out packet);
                }

                if (packet != null)
                {
                    ProcessPacket(packet);
                }

                if (this.QueueLogicPacket.IsEmpty)
                {
                    this.evFlag.WaitOne();
                }
            }
        }
    }
}
