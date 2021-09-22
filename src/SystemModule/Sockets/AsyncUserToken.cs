using System;
using System.Net;
using System.Net.Sockets;

namespace SystemModule.Sockets
{
    /// <summary>
    /// ??????????????????????SocketAsyncEventArgs.UserToken???????.
    /// </summary>
    public class AsyncUserToken : EventArgs
    {
        private Socket m_socket;//Socket
        private string m_connectionId;//???????ID
        private int m_SocketIndex;//��????
        private IPEndPoint m_endPoint;//????
        private byte[] m_receiveBuffer;//??????
        private int m_count;
        private int m_offset;//?????
        private int m_bytesReceived;//???????????????
        private SocketAsyncEventArgs m_readEventArgs;// SocketAsyncEventArgs??????
        private object m_operation;

        public AsyncUserToken()
            : this(null)
        {
        }

        /// <summary>
        /// ??????????? ????????��????????????????(?????��? ????????????????????)
        /// </summary>
        public object Operation
        {
            set { m_operation = value; }
            get { return m_operation; }
        }

        /// <summary>
        /// ?????????
        /// </summary>
        public byte[] ReceiveBuffer
        {
            get { return m_receiveBuffer; }
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        public int Offset
        {
            get { return m_offset; }
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        public int BytesReceived
        {
            get { return m_bytesReceived; }
        }

        /// <summary>
        /// ?????????SocketAsyncEventArgs??????
        /// </summary>
        public SocketAsyncEventArgs ReadEventArgs
        {
            set { m_readEventArgs = value; }
            get { return m_readEventArgs; }
        }

        /// <summary>
        /// ��????Scoket??????
        /// </summary>
        /// <param name="socket">Socket??????</param>
        public AsyncUserToken(Socket socket)
        {
            m_readEventArgs = new SocketAsyncEventArgs();
            m_readEventArgs.UserToken = this;
            if (null != socket)
            {
                m_socket = socket;
                this.m_endPoint = (IPEndPoint)socket.RemoteEndPoint;
            }
        }

        /// <summary>
        /// ?????????��????Socket??????
        /// </summary>
        public Socket Socket
        {
            get { return m_socket; }
            set
            {
                if (value != null)
                {
                    m_socket = value;
                    m_endPoint = (IPEndPoint)m_socket.RemoteEndPoint;
                }
            }
        }

        /// <summary>
        /// ?????????????????????ID??
        /// </summary>
        public string ConnectionId//???????ID
        {
            get { return this.m_connectionId; }
            set { this.m_connectionId = value; }
        }

        /// <summary>
        /// ?????????Socket��????
        /// </summary>
        public int nIndex
        {
            get { return this.m_SocketIndex; }
            set { this.m_SocketIndex = value; }
        }

        /// <summary>
        /// ??????????????????????
        /// </summary>
        public IPEndPoint EndPoint//???????
        {
            get { return this.m_endPoint; }
        }

        /// <summary>
        /// ???????????IP???
        /// </summary>
        public string RemoteIPaddr
        {
            get
            {
                return EndPoint.Address.ToString();
            }
        }
        
        public int RemotePort
        {
            get
            {
                return EndPoint.Port;
            }
        }

        /// <summary>
        /// ????????????????????????????��??
        /// </summary>
        /// <param name="bytesReceived">????????????</param>
        public void SetBytesReceived(int bytesReceived)
        {
            m_bytesReceived = bytesReceived;
        }

        /// <summary>
        /// ????????????????????????????��??
        /// </summary>
        /// <param name="buffer">????????????????��??</param>
        /// <param name="offset">?????????????????</param>
        /// <param name="bytesReceived">????????????</param>
        public void SetBuffer(byte[] buffer, int offset, int count)
        {
            m_receiveBuffer = buffer;
            m_offset = offset;
            m_count = count;
            m_bytesReceived = 0;
        }
    }
}