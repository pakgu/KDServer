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
        static void Main(string[] args)
        {
            CServer.GetInstance().Init();

            Console.WriteLine("Server On");

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }

            Console.ReadKey();
        }
    }
}
