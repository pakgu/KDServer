using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDServer.Net
{
    class CPacketBufferManager
    {
        static object cs_pool = new object();
        static Stack<CPacket> pool;
        static int capacity;

        public static void Init(int _capacity)
        {
            pool = new Stack<CPacket>();
            capacity = _capacity;
            Allocate();
        }

        static void Allocate()
        {
            for (int i = 0; i < capacity; ++i)
            {
                pool.Push(new CPacket());
            }
        }

        public static CPacket pop()
        {
            lock (cs_pool)
            {
                if (pool.Count <= 0)
                {
                    Allocate();
                    Console.WriteLine("reAllocate");
                }

                return pool.Pop();
            }
        }

        public static void push(CPacket packet)
        {
            lock (cs_pool)
            {
                pool.Push(packet);
            }
        }
    }
}
