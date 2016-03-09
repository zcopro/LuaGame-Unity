using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using FGame.Network;
using SLua;
using FGame.Common;

namespace FGame.Manager
{
    [CustomLuaClass]
    public enum Protocal
    {
        Connect         = 1,	//连接服务器
        Exception,	            //异常掉线
        Disconnect,             //正常断线  
        Timeout,
        Ping,
        GameData,
    }

    [CustomLuaClass]
    public class NetworkManager : MonoBehaviour {
        public string MethodName = "OnReceiveMessage";

        private static NetworkManager _instance = null;
        public static NetworkManager Instance
        {
            get {
                if(null == _instance)
                {
                    GameObject go = new GameObject("NetworkManager");
                    _instance = go.AddComponent<NetworkManager>();
                }
                return _instance;
            }
        }
        public void TouchInstance()
        {  }

        Queue<KeyValuePair<Protocal, ByteBuffer>> sEvents = new Queue<KeyValuePair<Protocal, ByteBuffer>>();
#if USE_SUPER_SOCKET
        SuperSocket.ClientEngine.FSuperSocket m_SuperSocket;
        public SuperSocket.ClientEngine.FSuperSocket LogicSocket
        {
            get {
                if (m_SuperSocket == null)
                {
                    m_SuperSocket = new SuperSocket.ClientEngine.FSuperSocket(this);
                }
                return m_SuperSocket;
            }
        }
#elif USE_TCPCLIENT
        SocketClient m_SocketClient;
        public SocketClient LogicSocket
        {
            get
            {
                if(m_SocketClient == null)
                {
                    m_SocketClient = new SocketClient(this);
                }
                return m_SocketClient;
            }
        }
#else
        FSocketLogic m_SocketLogic;
        public FSocketLogic LogicSocket
        {
            get {
                if (m_SocketLogic == null)
                {
                    m_SocketLogic = new FSocketLogic(this);
                }
                return m_SocketLogic;
            }
        }
#endif
        void Awake() {
            DontDestroyOnLoad(gameObject);
#if USE_SUPER_SOCKET
            LogUtil.Log("USE_SUPER_SOCKET");
#elif USE_TCPCLIENT
            LogUtil.Log("USE_TCPCLIENT");
#else
            LogUtil.Log("USE_FSOCKET");
#endif
        }

        void OnDestroy()
        {
#if USE_SUPER_SOCKET
            LogicSocket.Close();
#elif USE_TCPCLIENT
            LogicSocket.OnRemove();
#else
            LogicSocket.Close();
#endif
        }

        [DoNotToLua]
        public void AddEvent(Protocal id, ByteBuffer data)
        {
            sEvents.Enqueue(new KeyValuePair<Protocal, ByteBuffer>(id, data));
        }

        protected void CallMethod(string funcname,params object[] args)
        {
            if (null == LuaSvr.mainLuaState || null == LuaSvr.mainLuaState.luaState)
                return;
            LuaState l = LuaSvr.mainLuaState.luaState;
            LuaFunction func = l.getFunction(funcname);
            if (null != func)
            {
                func.call(args);
                func.Dispose();
            }
            else
            {
                LogUtil.LogWarning("function {0} is not exits.",funcname);
            }
        }

        /// <summary>
        /// 交给Command，这里不想关心发给谁。
        /// </summary>
        void Update()
        {
            if (sEvents.Count > 0)
            {
                while (sEvents.Count > 0)
                {
                    KeyValuePair<Protocal, ByteBuffer> _event = sEvents.Dequeue();
                    CallMethod(MethodName, new object[] { _event.Key, _event.Value });
                }
            }
        }


        public void ConnectTo(string host, int port)
        {
            LogicSocket.ConnectTo(host, port);
        }

        public void Close()
        {
            LogicSocket.Close();
        }

        public void SendMessage(ByteBuffer buffer)
        {
#if USE_SUPER_SOCKET
            LogicSocket.Send(buffer.ToBytes());
            buffer.Close();
#elif USE_TCPCLIENT
            LogicSocket.SendMessage(buffer);
#else
            LogicSocket.Send(buffer.ToBytes());
            buffer.Close();
#endif
        }

        public bool IsConnected
        {
            get
            {
                return LogicSocket.IsConnected;
            }
        }

        public void Ping(string ip)
        {
            new System.Threading.Thread(() =>
            {
                try
                {
                    System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
                    string data = "Hello!";
                    byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data);
                    int timeout = 1000; // Timeout 时间，单位：毫秒  
                    p.PingCompleted += new System.Net.NetworkInformation.PingCompletedEventHandler((object sender, System.Net.NetworkInformation.PingCompletedEventArgs e) =>
                    {
                        try
                        {
                            if(e.Error != null)
                            {
                                ByteBuffer buffer2 = new ByteBuffer();
                                buffer2.WriteString(string.Format("Ping:{0},Error:{1}", ip,e.Error.Message));
                                AddEvent(Protocal.Ping, new ByteBuffer(buffer2.ToBytes()));
                            }
                            System.Net.NetworkInformation.PingReply reply = e.Reply;
                            if (reply != null && reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                            {
                                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                                sb.AppendFormat("AcceptHost:{0}\n", reply.Address.ToString());
                                sb.AppendFormat("RoundTime:{0}\n", reply.RoundtripTime);
                                sb.AppendFormat("TTL:{0}", reply.Options.Ttl);
                                sb.AppendFormat("DontFragment:{0}", reply.Options.DontFragment);
                                sb.AppendFormat("Length:{0}", reply.Buffer.Length);
                                ByteBuffer buffer2 = new ByteBuffer();
                                buffer2.WriteString(sb.ToString());
                                AddEvent(Protocal.Ping, new ByteBuffer(buffer2.ToBytes()));
                            }
                            else
                            {
                                ByteBuffer buffer2 = new ByteBuffer();
                                buffer2.WriteString(string.Format("Ping:{0},Cannot Reachable", ip));
                                AddEvent(Protocal.Ping, new ByteBuffer(buffer2.ToBytes()));
                            }
                        }
                        catch (Exception ex)
                        {
                            ByteBuffer buffer2 = new ByteBuffer();
                            buffer2.WriteString(string.Format("Ping:{0},Error:{1}", ip, ex.Message));
                            AddEvent(Protocal.Ping, new ByteBuffer(buffer2.ToBytes()));
                        }
                    });
                    p.SendAsync(ip, timeout, buffer, null);
                }
                catch (Exception ex)
                {
                    ByteBuffer buffer2 = new ByteBuffer();
                    buffer2.WriteString(string.Format("Ping:{0},Error:{1}", ip, ex.Message));
                    AddEvent(Protocal.Ping, new ByteBuffer(buffer2.ToBytes()));
                }
            }).Start();
        }

    }
}