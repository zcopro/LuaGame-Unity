using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System;
using System.Threading;
using System.IO;

namespace FGame.Network
{
    public class FSocket
    {
        private Socket m_clientSocket;
        private string m_host;
        private int m_port;
        private bool m_connecting = false;
        private bool m_connected = false;
        private ManualResetEvent m_TimeoutObject = new ManualResetEvent(false);
        private int m_timeoutMiliseconds = 5000;
        private int m_connectCounter = 0;
        private const int MAX_READ = 2048;
#if ANSY_RECEIVE
        private byte[] m_RecvBuff = new byte[MAX_READ];
#else
        private MemoryStream mReaderStream = null;
        private Thread m_hThreadRecv = null;
        private bool m_bExited = false;
#endif
        private object m_lockObj = new object();

        public enum DisConnectReason
        {
            Disconnect = 1,
            Exception = 2,
        };

        public FSocket()
        {
         
        }
        public FSocket(string host,int port)
        {
            InitAddress(host, port);
        }

        public void InitAddress(string host,int port)
        {
            this.m_host = host;
            this.m_port = port;
        }

        public bool IsConnected
        {
            get
            {
                return !m_connecting && m_connected && m_clientSocket != null && m_clientSocket.Connected;
            }
        }

        public int Timeout
        {
            get { return m_timeoutMiliseconds; }
            set { m_timeoutMiliseconds = value; }
        }

        public int ConnectTimes
        {
            get { return m_connectCounter; }
        }

        public bool IsConnecting
        {
            get { return m_connecting; }
        }

        public void Connect()
        {
            try
            {
                Close();
                m_TimeoutObject.Reset();
                m_connecting = true;
                m_connectCounter++;
                m_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_clientSocket.ReceiveTimeout = Timeout;
                new Thread(() =>
                {
                    try
                    {
                        m_clientSocket.BeginConnect(m_host, m_port, new AsyncCallback(connectCallback), this);
                        if (m_TimeoutObject.WaitOne(Timeout, false))
                        {
                            if (!IsConnected && IsConnecting)
                            {
                                timeoutCallback();
                            }
                        }                  
                    }
                    catch(Exception e)
                    {
                        Close();
                        LogUtil.LogWarning("BeginConnect:" + e.Message);
                    }
                }).Start();
            }
            catch(Exception e)
            {
                disconnectCallback(DisConnectReason.Exception);
                LogUtil.LogWarning("Connect:" + e.Message);
            }
        }

        public void Close()
        {
            try
            {
                m_connectCounter = 0;
                m_connected = false;
                m_connecting = false;

                if (IsConnected)
                {
                    m_clientSocket.Shutdown(SocketShutdown.Both);
                    m_clientSocket.Close(1);
                    m_clientSocket = null;  
                }
#if !ANSY_RECEIVE
                m_bExited = true;
                if(mReaderStream != null)
                {
                    mReaderStream.Close();
                    mReaderStream = null;
                }
                if (m_hThreadRecv != null)
                {
                    m_hThreadRecv.Abort();
                    m_hThreadRecv.Join();
                    m_hThreadRecv = null;
                }
#endif
                LogUtil.LogWarning("Close FSocket...");
            }
            catch (Exception e)
            {
                LogUtil.LogWarning("Close:" + e.Message);
            }
            finally
            {
            }
        }

        public void Send(byte[] data)
        {
            if(!IsConnected)
            {
                return;
            }
            try
            {
                MemoryStream ms = null;
                using (ms = new MemoryStream())
                {
                    ms.Position = 0;
                    BinaryWriter writer = new BinaryWriter(ms);
                    ushort msglen = (ushort)data.Length;
                    writer.Write(msglen);
                    writer.Write(data);
                    writer.Flush();
                    byte[] payload = ms.ToArray();
                    m_clientSocket.BeginSend(payload, 0, payload.Length, SocketFlags.None, new AsyncCallback(sendCallback), this);
                    //LogUtil.Log("Send binary data:" + FGame.Utility.Util.ToHexString(payload));
                }
            }
            catch(Exception e)
            {
                LogUtil.LogWarning("Send:" + e.Message);
            }
        }

        protected virtual void onConnect()
        {  }
        protected void connectCallback(IAsyncResult asr)
        {
            FSocket pThis = (FSocket)asr.AsyncState;
            try
            {                
                pThis.m_connectCounter = 0;
                pThis.m_connecting = false;
                Socket clientSocket = pThis.m_clientSocket;
                if (null != clientSocket)
                {
                    clientSocket.EndConnect(asr);
                    pThis.m_connected = true;
                    LogUtil.Log(string.Format("Success Connected To:{0}", clientSocket.RemoteEndPoint.ToString()));
                    pThis.onConnect();
#if ANSY_RECEIVE
                    try
                    {
                        pThis.m_clientSocket.BeginReceive(pThis.m_RecvBuff, 0, pThis.m_RecvBuff.Length, SocketFlags.None, new AsyncCallback(receiveCallback), pThis);     
                    }
                    catch(Exception e)
                    {
                        pThis.Close();
                        LogUtil.LogWarning("BeginReceive:" + e.Message);
                    }
#else
                    if(pThis.mReaderStream == null)
                        pThis.mReaderStream = new MemoryStream(MAX_READ);

                    pThis.m_bExited = false;
                    pThis.m_hThreadRecv = new Thread(new ParameterizedThreadStart(_RecvThread));
                    pThis.m_hThreadRecv.IsBackground = true;
                    pThis.m_hThreadRecv.Start(this);
#endif
                } 
                else
                {
                    pThis.m_connected = false;
                    pThis.disconnectCallback(DisConnectReason.Exception);
                    LogUtil.LogWarning("connectCallback: clientSocket is null");
                } 
            }
            catch(Exception e)
            {
                pThis.m_connected = false;
                pThis.disconnectCallback(DisConnectReason.Exception);
                LogUtil.LogWarning("connectCallback:" + e.Message);
            }
            finally
            {
                pThis.m_TimeoutObject.Set();
            }
        }

        protected virtual void onReceive(byte[] data,long nLength)
        {
        }
        
#if ANSY_RECEIVE
        protected void receiveCallback(IAsyncResult asr)
        {
            FSocket pThis = (FSocket)asr.AsyncState;
            try
            {
                Socket handler = pThis.m_clientSocket;
                int nRead = handler.EndReceive(asr);
                if(nRead>0)
                {
                    lock (pThis.m_lockObj)
                    {
                        pThis.onReceive(pThis.m_RecvBuff, nRead);
                    }
                    LogUtil.Log("receiveCallback: Length:" + nRead);
                }
                handler.BeginReceive(pThis.m_RecvBuff, 0, pThis.m_RecvBuff.Length, SocketFlags.None, new AsyncCallback(receiveCallback), pThis);     
            }
            catch (Exception e)
            {
                pThis.Close();
                pThis.disconnectCallback(DisConnectReason.Disconnect);
                LogUtil.LogWarning("receiveCallback" + e.Message);
            }
        }
#else
        protected void _RecvThread(object o)
        {
            FSocket pThis = (FSocket)o;
            while (true)
            {
                if(pThis.m_bExited)
                {
                    break;
                }
                if(!pThis.IsConnected)
                {
                    pThis.Close();
                    pThis.disconnectCallback(DisConnectReason.Disconnect);
                    break;
                }
                try
                {     
                    if (pThis.m_clientSocket.Poll(-1, SelectMode.SelectRead))
                    {
                        byte[] buffer = new byte[MAX_READ];
                        int nRead = pThis.m_clientSocket.Receive(buffer);
                        if (nRead <= 0)
                        {
                            pThis.Close();
                            pThis.disconnectCallback(DisConnectReason.Disconnect);
                            break;
                        }
                        else
                        {
                            LogUtil.Log("Receive Length:{0}", nRead);
                            lock(pThis.m_lockObj)
                            {
                                try
                                {
                                    pThis.mReaderStream.Seek(0, SeekOrigin.End);
                                    pThis.mReaderStream.Write(buffer, 0, nRead);
                                    pThis.mReaderStream.Seek(0, SeekOrigin.Begin);
                                    if (nRead < MAX_READ)
                                    {
                                        long nLength = pThis.mReaderStream.Length;
                                        pThis.onReceive(pThis.mReaderStream.ToArray(), nLength);
                                        pThis.mReaderStream.SetLength(0); //Clear
                                    }
                                }
                                catch(Exception e)
                                {
                                    LogUtil.LogWarning("_RecvThread Lock:" + e.Message);
                                }
                                
                            }
                            Thread.Sleep(100);
                        }
                    }
                    else if (pThis.m_clientSocket.Poll(-1, SelectMode.SelectError))
                    {//Socket Error
                        pThis.Close();
                        pThis.disconnectCallback(DisConnectReason.Disconnect);
                        LogUtil.LogWarning("_RecvThread:" + "Socket Read Error.");
                        break;
                    }                          
                }
                catch(SocketException e)
                {
                    if (e.NativeErrorCode.Equals(10035))
                    {
                        //仍然处于连接状态,但是发送可能被阻塞
                    }
                    else
                    {
                        //连接错误,返回错误代码:e.NativeErrorCode
                        pThis.Close();
                        pThis.disconnectCallback(DisConnectReason.Disconnect);
                        LogUtil.LogWarning("_RecvThread:" + e.NativeErrorCode.ToString());
                        break;
                    }
                }
                catch(Exception e)
                {
                    pThis.Close();
                    pThis.disconnectCallback(DisConnectReason.Disconnect);
                    LogUtil.LogWarning("_RecvThread:" + e.Message);
                    break;
                }
            }
        }
#endif

        protected virtual void onDisconnect(DisConnectReason reason)
        { }

        protected void disconnectCallback(DisConnectReason reason)
        {
            LogUtil.LogWarning("disconnectCallback:" + reason);
            onDisconnect(reason);
        }

        protected virtual void onTimeout()
        { }
        protected void timeoutCallback()
        {
            LogUtil.LogWarning("Failed Connect,Reason=Timeout");
            onTimeout();
        }

        protected void sendCallback(IAsyncResult asr)
        {
            FSocket pThis = (FSocket)asr.AsyncState;
            try
            {
                Socket handler = pThis.m_clientSocket;
                int bytesSent = handler.EndSend(asr);
                Debug.LogFormat("Sent {0} bytes to Server.", bytesSent);
            }
            catch (Exception e)
            {
                LogUtil.LogWarning("sendCallback:" + e.Message);
            }
        }
    }
}
