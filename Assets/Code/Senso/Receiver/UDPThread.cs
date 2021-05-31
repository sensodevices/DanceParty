using UnityEngine;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Senso
{
    public class UDPThread : NetworkThread
    {
        private Thread netThread;

        private UdpClient m_sock;
        private IPEndPoint ep;
        
        private System.Object sendLock = new System.Object();

        private int SEND_BUFFER_SIZE = 4096; //!< Size of the buffer to send
        private Byte[] outBuffer;
        private int outBufferOffset = 0;

        private NetData[] pendingPackets;
        private int pendingPacketsWriteInd = 0;
        private int pendingPacketsReadInd = 0;

        ///
        /// @brief Default constructor
        ///
        public UDPThread(string host, Int32 port) : base(host, port)
        {
            outBuffer = new Byte[SEND_BUFFER_SIZE];
            pendingPackets = new NetData[2 * MAX_PACKET_CNT];

            if (State != NetworkState.SENSO_ERROR)
            {
                ep = new IPEndPoint(m_ip, m_port);
            }
        }

        ~UDPThread()
        {
            StopThread();
        }

        ///
        /// @brief starts the thread that reads from socket
        ///
        public override void StartThread()
        {
            if (!m_isStarted)
            {
                m_isStarted = true;
                netThread = new Thread(Run);
                netThread.Start();
            }
        }

        ///
        /// @brief Stops the thread that reads from socket
        ///
        public override void StopThread()
        {
            if (m_isStarted)
            {
                m_isStarted = false;
                netThread.Join();
            }
        }

        private void Run()
        { 
            Byte[] inBuffer;
            m_sock = new UdpClient();
            var outBufferCopy = new Byte[SEND_BUFFER_SIZE];
            int outLen;

            while (m_isStarted && State != NetworkState.SENSO_ERROR)
            {
                try
                {
                    var now = DateTime.Now;

                    bool rcvReady = false;
                    while (m_isStarted && !rcvReady)
                    {
                        rcvReady = m_sock.Client.Poll(10, SelectMode.SelectRead);
                        if (!rcvReady && DateTime.Now.Subtract(now).Milliseconds >= 200) break;
                    }
                    if (rcvReady)
                    {
                        inBuffer = m_sock.Receive(ref ep);
                        int packetStart = 0;
                        for (int i = 0; i < inBuffer.Length; ++i)
                        {
                            if (inBuffer[i] == '\n')
                            {
                                if (State == NetworkState.SENSO_CONNECTING) State = NetworkState.SENSO_CONNECTED;
                                var packet = processJsonStr(Encoding.ASCII.GetString(inBuffer, packetStart, i - packetStart));
                                if (packet != null)
                                {
                                    pendingPackets[pendingPacketsWriteInd] = packet;
                                    if (pendingPacketsWriteInd < pendingPackets.Length - 1)
                                    {
                                        Interlocked.Increment(ref pendingPacketsWriteInd);
                                    }
                                    else
                                    {
                                        Interlocked.Exchange(ref pendingPacketsWriteInd, 0);
                                    }
                                    if (pendingPacketsWriteInd == pendingPacketsReadInd) Interlocked.Increment(ref pendingPacketsReadInd);
                                }
                                packetStart = i + 1;
                            }
                        }
                    }
                    if (outBufferOffset > 0)
                    {
                        lock (sendLock)
                        {
                            Buffer.BlockCopy(outBuffer, 0, outBufferCopy, 0, outBufferOffset);
                            outLen = outBufferOffset;
                            outBufferOffset = 0;
                        }
                        for (int i = 0; i < outLen; ++i)
                        {
                            if (outBufferCopy[i] == '\n')
                            {
                                m_sock.Send(outBufferCopy, i, ep);
                                Buffer.BlockCopy(outBufferCopy, i + 1, outBufferCopy, 0, outLen - i);
                                outLen -= i;
                            }
                        }
                        if (outLen > 0)
                        {
                            m_sock.Send(outBufferCopy, outLen, ep);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    Debug.LogError("(Socket) Unable to get packet from Senso with code " + ex.ErrorCode + ": " + ex.Message);
                    //State = NetworkState.SENSO_ERROR;
                }
                catch (Exception ex)
                {
                    Debug.LogError("(General) Unable to get packet from Senso: " + ex.Message);
                }
            }
            Debug.Log("(Socket) end");
            m_sock.Close();
            m_sock = null;
            State = NetworkState.SENSO_DISCONNECTED;
        }

        public override int UpdateData(ref NetData[] res)
        {
            int ri = pendingPacketsReadInd, wi = pendingPacketsWriteInd;
            Interlocked.Exchange(ref pendingPacketsReadInd, wi);
            int unreadPackets;
            if (wi < ri)
            {
                unreadPackets = (pendingPackets.Length - ri) + wi;
            } else
            {
                unreadPackets = wi - ri;
            }
            if (res.Length < unreadPackets) unreadPackets = res.Length;
            
            for (int cnt = 0, i = wi - 1; cnt < unreadPackets; ++cnt)
            {
                if (i < 0) i = pendingPackets.Length - 1;
                //Debug.Log(i);
                res[cnt] = pendingPackets[i];
                --i;
            }
            return unreadPackets;
        }


        ///
        /// @brief Send vibrating command to the server
        ///
        public override void VibrateFinger(EPositionType handType, EFingerType fingerType, ushort duration, byte strength)
        {
            if (m_sock != null)
            {
                sendToServer(GetVibrateFingerJSON(handType, fingerType, duration, strength));
            }
        }

        ///
        /// @brief Sends HMD orientation to Senso Server
        ///
        public override void SetHeadLocationAndRotation(Vector3 position, Quaternion rotation)
        {
            if (m_sock != null)
            {
                sendToServer(GetHeadLocationAndRotationJSON(position, rotation));
            }
        }

        ///
        /// @brief Sends ping to Senso Server
        ///
        public override void SendPing()
        {
            if (m_sock != null)
            {
                sendToServer(GetPingJSON());
            }
        }

        private void sendToServer(String str)
        {
            lock(sendLock)
                outBufferOffset += Encoding.ASCII.GetBytes(str, 0, str.Length, outBuffer, outBufferOffset);
        }
    }
}
