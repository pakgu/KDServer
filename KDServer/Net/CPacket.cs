using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDServer.Net
{
    public class CPacket
    {
        public IPeer owner { get; private set; }
        public byte[] buffer { get; private set; }
        public int position { get; private set; }
        public Int16 eventID { get; private set; }

        public static CPacket Create(Int16 eventID)
        {
            //CPacket packet = new CPacket();
            CPacket packet = CPacketBufferManager.pop();
            packet.SetEvnetID(eventID);
            return packet;
        }

        public static void Destroy(CPacket packet)
        {
            CPacketBufferManager.push(packet);
        }

        public CPacket(byte[] buffer, IPeer owner)
        {
            this.buffer = buffer;

            // 헤더는 읽을필요 없으니 그 이후부터 시작한다.
            this.position = Define.HEADER_SIZE;

            this.owner = owner;
        }

        public CPacket()
        {
            this.buffer = new byte[Define.BUFFER_SIZE];
        }


        public void SetEvnetID(Int16 _eventID)
        {
            this.eventID = _eventID;
            
            // 헤더는 나중에 넣을것이므로 데이터 부터 넣을 수 있도록 위치를 점프시켜놓는다.
            this.position = Define.HEADER_SIZE;

            Put(_eventID);
        }

        public Int16 PopEventID()
        {
            return Get_int16();
        }

        public void Copy_to(CPacket _packet)
        {
            _packet.SetEvnetID(this.eventID);
            _packet.Write(this.buffer, this.position);
        }

        public void Write(byte[] source, int position)
        {
            Array.Copy(source, this.buffer, source.Length);
            this.position = position;
        }

        public void RecordSize()
        {
            Int16 dataSize = (Int16)(this.position - Define.HEADER_SIZE);
            byte[] header = BitConverter.GetBytes(dataSize);
            header.CopyTo(this.buffer, 0);
        }



        // -- Put --
        public void Put(byte data)
        {
            byte[] temp_buffer = BitConverter.GetBytes(data);
            temp_buffer.CopyTo(this.buffer, this.position);
            this.position += temp_buffer.Length;
        }

        public void Put(Int16 data)
        {
            byte[] temp_buffer = BitConverter.GetBytes(data);
            temp_buffer.CopyTo(this.buffer, this.position);
            this.position += temp_buffer.Length;
        }

        public void Put(Int32 data)
        {
            byte[] temp_buffer = BitConverter.GetBytes(data);
            temp_buffer.CopyTo(this.buffer, this.position);
            this.position += temp_buffer.Length;
        }

        public void Put(string data)
        {
            byte[] temp_buffer = Encoding.UTF8.GetBytes(data);

            Int16 len = (Int16)temp_buffer.Length;
            byte[] len_buffer = BitConverter.GetBytes(len);
            len_buffer.CopyTo(this.buffer, this.position);
            this.position += len_buffer.Length;

            temp_buffer.CopyTo(this.buffer, this.position);
            this.position += temp_buffer.Length;
        }


        // -- Get --
        public byte Get_byte()
        {
            byte data = (byte)BitConverter.ToInt16(this.buffer, this.position);
            this.position += sizeof(byte);
            return data;
        }

        public Int16 Get_int16()
        {
            Int16 data = BitConverter.ToInt16(this.buffer, this.position);
            this.position += sizeof(Int16);
            return data;
        }

        public Int32 Get_int32()
        {
            Int32 data = BitConverter.ToInt32(this.buffer, this.position);
            this.position += sizeof(Int32);
            return data;
        }

        public string Get_string()
        {
            Int16 len = BitConverter.ToInt16(this.buffer, this.position);
            this.position += sizeof(Int16);

            // 인코딩은 utf8로 통일한다.
            string data = System.Text.Encoding.UTF8.GetString(this.buffer, this.position, len);
            this.position += len;

            return data;
        }
    }
}
