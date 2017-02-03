using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KDServer.Net;

namespace KDServer
{
    class Program
    {
        UserManager userManager;

        static void Main(string[] args)
        {
            CPacketBufferManager.Init(2000);

            CNetworkService service = new CNetworkService();            

            service.callback_CreatedSession += On_CreatedSession;

            service.Init();
            service.Listen("0.0.0.0", 7979, 100);

            this.userManager.Init();

            Console.WriteLine("Server On");

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }

            Console.ReadKey();
        }

        static void On_CreatedSession(CUserToken token)
        {
            CUser user = new CUser(token);
            this.userManager.AddUser(user);
        }


    }
}
