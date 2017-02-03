using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace KDServer.Net
{
    class SocketAsyncEventArgsPool
    {
        Stack<SocketAsyncEventArgs> m_pool;

        public SocketAsyncEventArgsPool(int capacity)
        {
            m_pool = new Stack<SocketAsyncEventArgs>(capacity);
        }


        public void Push(SocketAsyncEventArgs args)
        {
            if (args == null) 
            {
                throw new ArgumentNullException("Push SocketAsyncEventArgsPool Fail. SocketAsyncEventArgs is Null"); 
            }

            lock (m_pool)
            {
                m_pool.Push(args);
            }
        }

        public SocketAsyncEventArgs Pop()
        {
            lock (m_pool)
            {
                return m_pool.Pop();
            }
        }

        public int Count
        {
            get { return m_pool.Count; }
        }
    }
}
