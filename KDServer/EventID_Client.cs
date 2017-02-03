using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDServer
{
    public enum EVENTID : short
    {
        E_BEGIN = 0,
        
        E_REQ,
        E_ACK,

        E_END
    }
}
