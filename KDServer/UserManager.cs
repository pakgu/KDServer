using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDServer
{
    public class UserManager
    {
        static List<CUser> userlist;

        public UserManager()
        {
        }

        public void Init()
        {
            userlist = new List<CUser>();
        }

        public void AddUser(CUser user)
        {
            lock (userlist)
            {
                userlist.Add(user);
            }
        }

        public void RemoveUser(CUser user)
        {
            lock (userlist)
            {
                userlist.Remove(user);
            }
        }
    }
}
