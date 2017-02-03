using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace KDServer.Net
{
    /// 서버와 클라이언트에서 공통으로 사용하는 세션 객체.
    public interface IPeer
    {
        void SendPacket(CPacket _packet);
        
        void OnPacket(Const<byte[]> buffer);
        
        void ProcessPacket(CPacket _packet);

        /// 원격 연결이 끊겼을 때 호출 된다.
        void OnRemoved();

        void Disconnect();
    }
}
 
