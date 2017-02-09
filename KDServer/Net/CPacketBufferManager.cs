using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace KDServer.Net
{
    class CPacketBufferManager
    {
        static ConcurrentStack<CPacket> pool;
        static int capacity;

        public static void Init(int _capacity)
        {
            pool = new ConcurrentStack<CPacket>();
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
            if (pool.IsEmpty)
            {
                Allocate();
                Console.WriteLine("Allocate PacketBuffPool");
            }

            CPacket packet;
            pool.TryPop(out packet);
            return packet;
        }

        public static void push(CPacket packet)
        {
            pool.Push(packet);
        }
    }
}
