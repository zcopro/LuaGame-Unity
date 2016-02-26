using FGame.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FGame.Network
{
    public class FSocketTest : FSocket
    {
        public FSocketTest(string host, int port) : base(host, port) { }
        protected override void onConnect()
        {
            base.onConnect();

            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteShort(104);
            buffer.WriteByte(0);
            buffer.WriteString("ffff鎴戠殑ffffQ闈坲uu");
            buffer.WriteInt(200);

            this.Send(buffer.ToBytes());
            buffer.Close();
        }

        bool bSend = false;

        protected override void onReceive(byte[] data, long nLength)
        {
            base.onReceive(data, nLength);
            if(!bSend)
            {
                bSend = true;
                ByteBuffer buffer = new ByteBuffer();
                buffer.WriteShort(104);
                buffer.WriteByte(0);
                buffer.WriteString("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                buffer.WriteInt(200);

                this.Send(buffer.ToBytes());
                buffer.Close();
            }
            
        }
    }
}