using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace KDServer
{
    class CUserManager
    {
        public ConcurrentBag<CUser> userlist { get; private set; }

        public CUserManager()
        {
            Init();
        }

        public void Init()
        {
            userlist = new ConcurrentBag<CUser>();
        }

        public void AddUser(CUser user)
        {
            userlist.Add(user);
        }

        public void RemoveUser(CUser user)
        {
            userlist.TryTake(out user);
        }
    }
}
