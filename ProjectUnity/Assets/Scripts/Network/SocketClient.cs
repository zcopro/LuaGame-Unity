using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using FGame.Manager;
using FGame.Common;

namespace FGame.Network
{
    public enum DisType
    {
        Exception,
        Disconnect,
    }

    public class SocketClient
    {
        private static ManualResetEvent TimeoutObject = new ManualResetEvent(false);
        private TcpClient client = null;
        private NetworkStream outStream = null;
        private MemoryStream memStream;
        private BinaryReader reader;

        private const int MAX_READ = 8192;
        private byte[] byteBuffer = new byte[MAX_READ];

        private NetworkManager NetMgr;

        // Use this for initialization
        public SocketClient(NetworkManager mgr)
        {
            NetMgr = mgr;
            OnRegister();
        }

        /// <summary>
        /// 注册代理
        /// </summary>
        void OnRegister()
        {
            memStream = new MemoryStream();
            reader = new BinaryReader(memStream);
        }

        /// <summary>
        /// 移除代理
        /// </summary>
        public void OnRemove()
        {
            this.Close();
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
            if (memStream != null)
            {
                memStream.Close();
                memStream = null;
            }
        }

        public bool IsConnected
        {
            get
            {
                return (client != null && client.Connected);
            }
        }
                
        /// <summary>
        /// 连接上服务器
        /// </summary>
        void OnConnect(IAsyncResult asr)
        {
            try
            {
                //TcpClient tcpclient = asr.AsyncState as TcpClient;
                //tcpclient.EndConnect(asr);
                outStream = client.GetStream();
                client.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
                ByteBuffer buffer = new ByteBuffer();
                buffer.WriteShort((ushort)Protocal.Connect);
                buffer.WriteString("Connected");
                NetMgr.AddEvent(Protocal.Connect, new ByteBuffer(buffer.ToBytes()));
                buffer.Close();
                LogUtil.Log("OnConnect " + outStream + "," + asr.IsCompleted);
            }
            finally
            {
                TimeoutObject.Set();
            }
        }

        /// <summary>
        /// 写数据
        /// </summary>
        void WriteMessage(byte[] message)
        {
            MemoryStream ms = null;
            using (ms = new MemoryStream())
            {
                ms.Position = 0;
                BinaryWriter writer = new BinaryWriter(ms);
                ushort msglen = (ushort)message.Length;
                writer.Write(msglen);
                writer.Write(message);
                writer.Flush();
                if (client != null && client.Connected)
                {
                    //NetworkStream stream = client.GetStream(); 
                    byte[] payload = ms.ToArray();
                    outStream.BeginWrite(payload, 0, payload.Length, new AsyncCallback(OnWrite), null);
                }
                else
                {
                    LogUtil.LogError("client.connected----->>false");
                }
            }
        }

        /// <summary>
        /// 读取消息
        /// </summary>
        void OnRead(IAsyncResult asr)
        {
            int bytesRead = 0;
            try
            {
                lock (client.GetStream())
                {         //读取字节流到缓冲区
                    try
                    {
                        bytesRead = client.GetStream().EndRead(asr);
                    }
                    catch(System.Exception ex)
                    {
                        OnDisconnected(DisType.Exception, ex.Message);
                        return;
                    }
                }
                if (bytesRead < 1)
                {                //包尺寸有问题，断线处理
                    OnDisconnected(DisType.Disconnect, "bytesRead < 1");
                    return;
                }
                OnReceive(byteBuffer, bytesRead);   //分析数据包内容，抛给逻辑层
                lock (client.GetStream())
                {         //分析完，再次监听服务器发过来的新消息
                    Array.Clear(byteBuffer, 0, byteBuffer.Length);   //清空数组
                    client.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
                }
            }
            catch (Exception ex)
            {
                //PrintBytes();
                OnDisconnected(DisType.Exception, ex.Message);
            }
        }

        /// <summary>
        /// 丢失链接
        /// </summary>
        void OnDisconnected(DisType dis, string msg)
        {
            Close();   //关掉客户端链接
            Protocal protocal = dis == DisType.Exception ? Protocal.Exception : Protocal.Disconnect;

            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteShort((ushort)protocal);
            buffer.WriteString(msg);
            NetMgr.AddEvent(protocal, new ByteBuffer(buffer.ToBytes()));
            buffer.Close();
            LogUtil.LogWarning("Connection was closed by the server:>" + msg + " Distype:>" + dis);
        }

        /// <summary>
        /// 打印字节
        /// </summary>
        /// <param name="bytes"></param>
        void PrintBytes()
        {
            string returnStr = string.Empty;
            for (int i = 0; i < byteBuffer.Length; i++)
            {
                returnStr += byteBuffer[i].ToString("X2");
            }
            LogUtil.LogError(returnStr);
        }

        /// <summary>
        /// 向链接写入数据流
        /// </summary>
        void OnWrite(IAsyncResult r)
        {
            try
            {
                outStream.EndWrite(r);
            }
            catch (Exception ex)
            {
                LogUtil.LogError("OnWrite--->>>" + ex.Message);
            }
        }

        /// <summary>
        /// 接收到消息
        /// </summary>
        void OnReceive(byte[] bytes, int length)
        {
            try
            {
                memStream.Seek(0, SeekOrigin.End);
                memStream.Write(bytes, 0, length);
                //Reset to beginning
                memStream.Seek(0, SeekOrigin.Begin);
                while (RemainingBytes() > 2)
                {
                    ushort messageLen = reader.ReadUInt16();
                    if (RemainingBytes() >= messageLen)
                    {
                        MemoryStream ms = new MemoryStream();
                        BinaryWriter writer = new BinaryWriter(ms);
                        writer.Write(reader.ReadBytes(messageLen));
                        ms.Seek(0, SeekOrigin.Begin);
                        OnReceivedMessage(ms);
                    }
                    else
                    {
                        //Back up the position two bytes
                        memStream.Position = memStream.Position - 2;
                        break;
                    }
                }
                //Create a new stream with any leftover bytes
                byte[] leftover = reader.ReadBytes((int)RemainingBytes());
                memStream.SetLength(0);     //Clear
                memStream.Write(leftover, 0, leftover.Length);
            }
            catch(Exception ex)
            {
                LogUtil.LogWarning(ex.Message);
            }
        }

        /// <summary>
        /// 剩余的字节
        /// </summary>
        private long RemainingBytes()
        {
            return memStream.Length - memStream.Position;
        }

        /// <summary>
        /// 接收到消息
        /// </summary>
        /// <param name="ms"></param>
        void OnReceivedMessage(MemoryStream ms)
        {
            BinaryReader r = new BinaryReader(ms);
            byte[] message = r.ReadBytes((int)(ms.Length - ms.Position));
            
            ByteBuffer buffer = new ByteBuffer(message);
            NetMgr.AddEvent(Protocal.GameData, buffer);
        }


        /// <summary>
        /// 会话发送
        /// </summary>
        void SessionSend(byte[] bytes)
        {
            WriteMessage(bytes);
        }

        /// <summary>
        /// 关闭链接
        /// </summary>
        public void Close()
        {
            if (client != null)
            {
                if (client.Connected) client.Close();
                client = null;
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public void SendMessage(ByteBuffer buffer)
        {
            SessionSend(buffer.ToBytes());
            buffer.Close();
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        public void ConnectTo(string host, int port)
        {
            client = new TcpClient();
            client.SendTimeout = 1000;
            client.ReceiveTimeout = 1000;
            client.NoDelay = true;
            try
            {
                TimeoutObject.Reset();
                new Thread(() =>
                {
                    client.BeginConnect(host, port, new AsyncCallback(OnConnect), null);
                    if (TimeoutObject.WaitOne(5000, false))
                    {
                        if (!IsConnected)
                        {
                            OnDisconnected(DisType.Disconnect, "timeout");
                        }
                    }
                }).Start();
                
            }
            catch (Exception e)
            {
                Close();
                OnDisconnected(DisType.Exception, e.Message);
                LogUtil.LogError(e.Message);
            }
        }

    }
}