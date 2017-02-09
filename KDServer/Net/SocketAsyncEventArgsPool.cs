using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace KDServer.Net
{
    class SocketAsyncEventArgsPool
    {
        ConcurrentStack<SocketAsyncEventArgs> pool;

        public SocketAsyncEventArgsPool()
        {
            pool = new ConcurrentStack<SocketAsyncEventArgs>();
        }


        public void Push(SocketAsyncEventArgs args)
        {
            if (args == null) 
            {
                throw new ArgumentNullException("Fail SocketAsyncEventArgsPool:Push"); 
            }

            pool.Push(args);
        }

        public SocketAsyncEventArgs Pop()
        {
            SocketAsyncEventArgs args;
            pool.TryPop(out args);
            return args;
        }

        public int Count
        {
            get { return pool.Count; }
        }
    }
}
