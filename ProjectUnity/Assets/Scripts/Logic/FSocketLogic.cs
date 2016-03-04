using UnityEngine;
using System.Collections;
using FGame.Network;
using FGame.Manager;
using FGame.Common;
using System;

namespace FGame
{
    public class FSocketLogic : FSocket
    {
        public FSocketLogic(NetworkManager mgr) : base() {
            NetMgr = mgr;
        }

        NetworkManager NetMgr;
        
        protected override void onConnect()
        {
            base.onConnect();
            NetMgr.AddEvent(Protocal.Connect, new ByteBuffer());
        }

        protected override void onReceive(byte[] data, long nLength)
        {
            base.onReceive(data, nLength);

            byte[] receiveData = new byte[nLength];
            Array.Copy(data,0,receiveData,0, nLength);

            ByteBuffer buffer = new ByteBuffer(receiveData);
            int nLen = (int)buffer.ReadShort();
            LogUtil.Log("FSocketLogic onReceive:" + nLen);
            //LogUtil.Log("onReceive binary data:" + Util.ToHexString(receiveData));
            NetMgr.AddEvent(Protocal.GameData, buffer);
        }

        protected override void onDisconnect(DisConnectReason reason)
        {
            base.onDisconnect(reason);
            NetMgr.AddEvent(reason == DisConnectReason.Exception ? Protocal.Exception : Protocal.Disconnect, new ByteBuffer());
        }

        protected override void onTimeout()
        {
            base.onTimeout();
            NetMgr.AddEvent(Protocal.Timeout, new ByteBuffer());
        }

        public void ConnectTo(string host,int port)
        {
            this.InitAddress(host, port);
            this.Connect();
        }
    }
}