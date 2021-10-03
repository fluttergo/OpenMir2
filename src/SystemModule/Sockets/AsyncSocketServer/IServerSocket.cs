using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SystemModule.Sockets
{
    /// <summary>
    /// 异步Socket通讯服务器类
    /// </summary>
    public class ISocketServer
    {
        // 缓冲区同步对象
        private Object m_bufferLock = new Object();
        // 读对象池同步对象
        private Object m_readPoolLock = new Object();
        // 写对象池同步对象
        private Object m_writePoolLock = new Object();
        // 接收数据事件对象集合
        Dictionary<string, AsyncUserToken> m_tokens;

        // 添加功能
        int m_numConnections;           // 设计同时处理的连接最大数
        int m_BufferSize;               // 用于每一个Socket I/O操作使用的缓冲区大小
        BufferManager m_bufferManager;  // 为所有Socket操作准备的一个大的可重用的缓冲区集合        
        const int opsToPreAlloc = 2;    // 需要分配空间的操作数目:为 读、写、接收等 操作预分配空间(接受操作可以不分配)
        Socket listenSocket;            // 用来侦听到达的连接请求的Socket
        // 读Socket操作的SocketAsyncEventArgs可重用对象池        
        SocketAsyncEventArgsPool m_readPool;
        // 写Socket操作的SocketAsyncEventArgs可重用对象池
        SocketAsyncEventArgsPool m_writePool;

        bool isActive = false;

        // 测试用
        long m_totalBytesRead;          // 服务器接收到的总字节数计数器
        long m_totalBytesWrite;         // 服务器发送的字节总数

        long m_numConnectedSockets;      // 连接到服务器的Socket总数
        Semaphore m_maxNumberAcceptedClients;//最大接受请求数信号量
        /// <summary>
        /// 获取已经连接的Socket总数
        /// </summary>
        public long NumConnectedSockets
        {
            get { return m_numConnectedSockets; }
        }
        /// <summary>
        /// 获取接收到的字节总数
        /// </summary>
        public long TotalBytesRead
        {
            get { return m_totalBytesRead; }
        }
        /// <summary>
        /// 获取发送的字节总数
        /// </summary>
        public long TotalBytesWrite
        {
            get { return m_totalBytesWrite; }
        }
        /// <summary>
        /// 客户端已经连接事件
        /// </summary>
        public event EventHandler<AsyncUserToken> OnClientConnect;        // 客户端已经连接事件        
        /// <summary>
        /// 客户端错误事件
        /// </summary>
        public event EventHandler<AsyncSocketErrorEventArgs> OnClientError;     // 客户端错误事件       
        /// <summary>
        /// 接收到数据事件
        /// </summary>
        public event EventHandler<AsyncUserToken> OnClientRead;           // 接收到数据事件
        /// <summary>
        /// 数据发送完成
        /// </summary>
        public event EventHandler<AsyncUserToken> OnDataSendCompleted;// 数据发送完成
        /// <summary>
        /// 客户端断开连接事件
        /// </summary>
        public event EventHandler<AsyncUserToken> OnClientDisconnect;     // 客户端断开连接事件
        
        /// <summary>
        /// 客户端是否在线
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns>在线返回true，否则返回false</returns>
        public bool IsOnline(string connectionId)
        {
            lock (((ICollection)this.m_tokens).SyncRoot)
            {
                if (!this.m_tokens.ContainsKey(connectionId))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public IList<AsyncUserToken> GetSockets()
        {
            lock (((ICollection)this.m_tokens).SyncRoot)
            {
                return this.m_tokens.Values.ToList();
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="numConnections">服务器允许连接的客户端总数</param>
        /// <param name="receiveBufferSize">接收缓冲区大小</param>
        public ISocketServer(int numConnections, int BufferSize)//构造函数
        {
            // 重置接收和发送字节总数
            m_totalBytesRead = 0;
            m_totalBytesWrite = 0;
            // 已??拥目突Ф俗苁?
            m_numConnectedSockets = 0;
            // 数据库设计连接容量
            m_numConnections = numConnections;
            // 接收缓冲区大小
            m_BufferSize = BufferSize;

            // 为最大数目Socket 同时能拥有高性能的读、写通讯表现而分配缓冲区空间

            m_bufferManager = new BufferManager(BufferSize * numConnections * opsToPreAlloc,
                BufferSize);
            // 读写池
            m_readPool = new SocketAsyncEventArgsPool(numConnections);
            m_writePool = new SocketAsyncEventArgsPool(numConnections);

            // 接收数据事件参数对象集合
            m_tokens = new Dictionary<string, AsyncUserToken>();

            // 初始信号量
            m_maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);
        }

        /// <summary>
        /// 用预分配的可重用缓冲区和上下文对象初始化服务器.        
        /// </summary>
        public void Init()
        {
            // 为所有的I/O操作分配一个大的字节缓冲区.目的是防止内存碎片的产生             
            m_bufferManager.InitBuffer();

            // 可重用的SocketAsyncEventArgs,用于重复接受客户端使用
            SocketAsyncEventArgs readWriteEventArg;
            AsyncUserToken token;
            // 预分配一个 SocketAsyncEventArgs 对象池

            // 预分配SocketAsyncEventArgs读对象池
            for (int i = 0; i < m_numConnections; i++)
            {
                // 预分配一个 可重用的SocketAsyncEventArgs 对象
                //readWriteEventArg = new SocketAsyncEventArgs();
                // 创建token时同时创建了一个和token关联的用于读数据的SocketAsyncEventArgs对象,
                // 并且创建的SocketAsyncEventArgs对象的UserToken属性值为该token
                token = new AsyncUserToken();
                //token.ReadEventArgs = readWriteEventArg;
                //readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                // 注册一个SocketAsyncEventArgs完成事件
                token.ReadEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                //readWriteEventArg.UserToken = new AsyncUserToken();

                // 从缓冲区池中分配一个字节缓冲区给 SocketAsyncEventArg 对象
                //m_bufferManager.SetBuffer(readWriteEventArg);
                m_bufferManager.SetBuffer(token.ReadEventArgs);
                //((AsyncUserToken)readWriteEventArg.UserToken).SetReceivedBytes(readWriteEventArg.Buffer, readWriteEventArg.Offset, 0);
                //设定接收缓冲区及偏移量
                token.SetBuffer(token.ReadEventArgs.Buffer, token.ReadEventArgs.Offset, token.ReadEventArgs.Count);
                // 添加一个 SocketAsyncEventArg 到池中
                //m_readPool.Push(readWriteEventArg);
                m_readPool.Push(token.ReadEventArgs);
            }
            // 预分配SocketAsyncEventArgs写对象池
            for (int i = 0; i < m_numConnections; i++)
            {
                // 预分配一个 可重用的SocketAsyncEventArgs 对象
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                readWriteEventArg.UserToken = null;

                // 从缓冲区池中分配一个字节缓冲区给 SocketAsyncEventArg 对象
                m_bufferManager.SetBuffer(readWriteEventArg);
                //readWriteEventArg.SetBuffer(null, 0, 0);

                // 添加一个 SocketAsyncEventArg 到池中
                m_writePool.Push(readWriteEventArg);
            }
        }

        /// <summary>
        /// 启动异步Socket服务
        /// 支持指定IP和端口
        /// </summary>
        /// <param name="Port"></param>
        public void Start(string ip, int port)
        {
            if (ip == "*" || ip == "all")
            {
                Start(port);
            }
            else
            {
                Start(new IPEndPoint(IPAddress.Parse(ip), port));
                isActive = true;
            }
        }

        /// <summary>
        /// 启动异步Scoket服务
        /// 绑定本机所有IP并指定端口
        /// </summary>
        /// <param name="port"></param>
        public void Start(int port)
        {
            Start(new IPEndPoint(IPAddress.Any, port));
            isActive = true;
        }

        public bool Active
        {
            get { return isActive; }
        }

        /// <summary>
        /// 启动异步Socket服务器
        /// </summary>
        /// <param name="localEndPoint">要绑定的本地终结点</param>
        public void Start(IPEndPoint localEndPoint)// 启动
        {
            try
            {
                // 创建一个侦听到达连接的Socket
                if (null != listenSocket)
                {
                    listenSocket.Close();
                }
                else
                {
                    listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    listenSocket.Bind(localEndPoint);
                }
                // 用一个侦听1000个连接的队列启动服务器
                listenSocket.NoDelay = false;
                listenSocket.Listen(1000);
            }
            catch (ObjectDisposedException)
            {

            }
            catch (SocketException ex)
            {
                //RaiseErrorEvent(null, exception);
                // 启动失败抛出启动失败异常
                if (ex.ErrorCode == (int)SocketError.AddressNotAvailable)
                {
                    throw new AsyncSocketException(ex.Message, ex);
                }
                else if (ex.ErrorCode == 48)
                {
                    throw new AsyncSocketException("Socket端口被占用", AsyncSocketErrorCode.ServerStartFailure);
                }
                else
                {
                    throw new AsyncSocketException("服务器启动失败", AsyncSocketErrorCode.ServerStartFailure);
                }
            }
            catch (Exception exception_debug)
            {
                Debug.WriteLine("调试：" + exception_debug.Message);
                throw exception_debug;
            }
            // 在侦听Socket上抛出接受委托.      

            StartAccept(null); // (第一次不采用可重用的SocketAsyncEventArgs对象来接受请求的Socket的Accept方式)

            Debug.WriteLine("服务器启动成功....");
        }

        /// <summary>
        /// 开始接受连接请求
        /// </summary>
        /// <param name="acceptEventArg"></param>
        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                // 由于上下文对象正在被使用Socket必须被清理
                acceptEventArg.AcceptSocket = null;
            }
            try
            {
                m_maxNumberAcceptedClients.WaitOne();// 对信号量进行一次P操作

                bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArg);
                if (!willRaiseEvent)
                {
                    ProcessAccept(acceptEventArg);
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException socketException)
            {
                RaiseErrorEvent(null, new AsyncSocketException("服务器接受客户端请求发生一次异常", socketException));
                // 接受客户端发生异常
                //throw new AsyncSocketException("服务器接受客户端请求发生一次异常", AsyncSocketErrorCode.ServerAcceptFailure);
            }
            catch (Exception exception_debug)
            {
                Debug.WriteLine("调试：" + exception_debug.Message);
                throw exception_debug;
            }
        }

        /// <summary>
        /// 这个方法是关联异步接受操作的回调方法并且当接收操作完成时被调用        
        /// </summary>
        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            AsyncUserToken token;
            Interlocked.Increment(ref m_numConnectedSockets);
            Debug.WriteLine(string.Format("客户端连接请求被接受. 有 {0} 个客户端连接到服务器",
                m_numConnectedSockets.ToString()));
            SocketAsyncEventArgs readEventArg;
            // 获得已经接受的客户端连接Socket并把它放到ReadEventArg对象的user token中
            lock (m_readPool)
            {
                readEventArg = m_readPool.Pop();
            }

            token = (AsyncUserToken)readEventArg.UserToken;
            // 把它放到ReadEventArg对象的user token中
            token.Socket = e.AcceptSocket;
            // 获得一个新的Guid 32位 "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
            token.ConnectionId = Guid.NewGuid().ToString("N");

            lock (((ICollection)this.m_tokens).SyncRoot)
            {
                this.m_tokens.Add(token.ConnectionId, token);// 添加到集合中
            }

            EventHandler<AsyncUserToken> handler = OnClientConnect;
            // 如果订户事件将为空(null)
            if (handler != null)
            {
                handler(this, token);// 抛出客户端连接事件
            }

            try
            {
                // 客户端一连接上就抛出一个接收委托给连接的Socket开始接收数据
                bool willRaiseEvent = token.Socket.ReceiveAsync(readEventArg);
                if (!willRaiseEvent)
                {
                    ProcessReceive(readEventArg);
                }
            }
            catch (ObjectDisposedException)
            {
                RaiseDisconnectedEvent(token);
            }
            catch (SocketException socketException)
            {
                if (socketException.ErrorCode == (int)SocketError.ConnectionReset)// 10054一个建立的连接被远程主机强行关闭
                {
                    RaiseDisconnectedEvent(token);// 引发断开连接事件
                }
                else
                {
                    RaiseErrorEvent(token, new AsyncSocketException("在SocketAsyncEventArgs对象上执行异步接收数据操作时发生SocketException异常", socketException));
                }
            }
            catch (Exception exception_debug)
            {
                Debug.WriteLine("调试：" + exception_debug.Message);
                throw exception_debug;
            }
            finally
            {
                // 接受下一个连接请求
                StartAccept(e);
            }
        }

        /// <summary>
        /// 这个方法在一个Socket读或者发送操作完成时被调用
        /// </summary> 
        /// <param name="e">和完成的接受操作关联的SocketAsyncEventArg对象</param>
        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // 确定刚刚完成的操作类型并调用关联的句柄
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("最后一次在Socket上的操作不是接收或者发送操作");
            }
        }

        /// <summary>
        /// 这个方法在异步接收操作完成时调用.  
        /// 如果远程主机关闭连接Socket将关闭        
        /// </summary>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            // 检查远程主机是否关闭连接
            if (e.SocketError != SocketError.Success)
            {
                Debug.WriteLine("asdasdasd");
            }
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                // 增加接收到的字节总数
                Interlocked.Add(ref m_totalBytesRead, e.BytesTransferred);
                Debug.WriteLine(string.Format("服务器读取字节总数:{0}", m_totalBytesRead.ToString()));

                //byte[] destinationArray = new byte[e.BytesTransferred];// 目的字节数组
                //Array.Copy(e.Buffer, 0, destinationArray, 0, e.BytesTransferred);
                token.SetBytesReceived(e.BytesTransferred);

                EventHandler<AsyncUserToken> handler = OnClientRead;
                // 如果订户事件将为空(null)
                if (handler != null)
                {
                    handler(this, token);// 抛出接收到数据事件                                                   
                }

                try
                {
                    // 继续接收数据
                    bool willRaiseEvent = token.Socket.ReceiveAsync(e);
                    if (!willRaiseEvent)
                    {
                        ProcessReceive(e);
                    }
                }
                catch (ObjectDisposedException)
                {
                    RaiseDisconnectedEvent(token);
                }
                catch (SocketException socketException)
                {
                    if (socketException.ErrorCode == (int)SocketError.ConnectionReset)//10054一个建立的连接被远程主机强行关闭
                    {
                        RaiseDisconnectedEvent(token);//引发断开连接事件
                    }
                    else
                    {
                        RaiseErrorEvent(token, new AsyncSocketException("在SocketAsyncEventArgs对象上执行异步接收数据操作时发生SocketException异常", socketException));
                    }
                }
                catch (Exception exception_debug)
                {
                    Debug.WriteLine("调试：" + exception_debug.Message);
                    throw exception_debug;
                }
            }
            else
            {
                RaiseDisconnectedEvent(token);
            }
        }

        public void Send(string connectionId, byte[] buffer)
        {
            AsyncUserToken token;
            //SocketAsyncEventArgs token;

            //if (buffer.Length <= m_receiveSendBufferSize)
            //{
            //    throw new ArgumentException("数据包长度超过缓冲区大小", "buffer");
            //}

            lock (((ICollection)this.m_tokens).SyncRoot)
            {
                if (!this.m_tokens.TryGetValue(connectionId, out token))
                {
                    throw new AsyncSocketException(string.Format("客户端:{0}已经关闭或者未连接", connectionId), AsyncSocketErrorCode.ClientSocketNoExist);
                    //return;
                }
            }
            SocketAsyncEventArgs writeEventArgs;
            lock (m_writePool)
            {
                writeEventArgs = m_writePool.Pop();// 分配一个写SocketAsyncEventArgs对象
            }
            writeEventArgs.UserToken = token;
            if (buffer.Length <= m_BufferSize)
            {
                Array.Copy(buffer, 0, writeEventArgs.Buffer, writeEventArgs.Offset, buffer.Length);
                writeEventArgs.SetBuffer(writeEventArgs.Buffer, writeEventArgs.Offset, buffer.Length);
            }
            else
            {
                lock (m_bufferLock)
                {
                    m_bufferManager.FreeBuffer(writeEventArgs);
                }
                writeEventArgs.SetBuffer(buffer, 0, buffer.Length);
            }

            //writeEventArgs.SetBuffer(buffer, 0, buffer.Length);
            try
            {
                // 异步发送数据
                bool willRaiseEvent = token.Socket.SendAsync(writeEventArgs);
                if (!willRaiseEvent)
                {
                    ProcessSend(writeEventArgs);
                }
            }
            catch (ObjectDisposedException)
            {
                RaiseDisconnectedEvent(token);
            }
            catch (SocketException socketException)
            {
                if (socketException.ErrorCode == (int)SocketError.ConnectionReset)//10054一个建立的?颖辉冻讨骰?啃泄乇蕴
                {
                    RaiseDisconnectedEvent(token);//引发断开连接事件
                }
                else
                {
                    RaiseErrorEvent(token, new AsyncSocketException("在SocketAsyncEventArgs对象上执行异步发送数据操作时发生SocketException异常", socketException)); ;
                }
            }
            catch (Exception exception_debug)
            {
                Debug.WriteLine("调试：" + exception_debug.Message);
                throw exception_debug;
            }
        }

        /// <summary>
        /// 发送数据,可以携带操作类型(使用自定义枚举来表示)
        /// </summary>
        /// <param name="connectionId">连接的ID号</param>
        /// <param name="buffer">缓冲区大小</param>
        /// <param name="operation">操作自定义枚举</param>
        public void Send(string connectionId, byte[] buffer, object operation)
        {
            AsyncUserToken token;
            //SocketAsyncEventArgs token;

            //if (buffer.Length <= m_receiveSendBufferSize)
            //{
            //    throw new ArgumentException("数据包长度超过缓冲区大小", "buffer");
            //}

            lock (((ICollection)this.m_tokens).SyncRoot)
            {
                if (!this.m_tokens.TryGetValue(connectionId, out token))
                {
                    throw new AsyncSocketException(string.Format("客户端:{0}已经关闭或者未连接", connectionId), AsyncSocketErrorCode.ClientSocketNoExist);
                    //return;
                }
            }
            SocketAsyncEventArgs writeEventArgs;
            lock (m_writePool)
            {
                writeEventArgs = m_writePool.Pop();// 分配一个写SocketAsyncEventArgs对象
            }
            writeEventArgs.UserToken = token;
            token.Operation = operation;// 设置操作标志
            if (buffer.Length <= m_BufferSize)
            {
                Array.Copy(buffer, 0, writeEventArgs.Buffer, writeEventArgs.Offset, buffer.Length);
            }
            else
            {
                lock (m_bufferLock)
                {
                    m_bufferManager.FreeBuffer(writeEventArgs);
                }
                writeEventArgs.SetBuffer(buffer, 0, buffer.Length);
            }

            //writeEventArgs.SetBuffer(buffer, 0, buffer.Length);
            try
            {
                // 异步发送数据
                bool willRaiseEvent = token.Socket.SendAsync(writeEventArgs);
                if (!willRaiseEvent)
                {
                    ProcessSend(writeEventArgs);
                }
            }
            catch (ObjectDisposedException)
            {
                RaiseDisconnectedEvent(token);
            }
            catch (SocketException socketException)
            {
                if (socketException.ErrorCode == (int)SocketError.ConnectionReset)//10054一个建立的连接被远程主机强行关闭
                {
                    RaiseDisconnectedEvent(token);//引发断开连接事件
                }
                else
                {
                    RaiseErrorEvent(token, new AsyncSocketException("在SocketAsyncEventArgs对象上执行异步发送数据操作时发生SocketException异常", socketException)); ;
                }
            }
            catch (Exception exception_debug)
            {
                Debug.WriteLine("调试：" + exception_debug.Message);
                throw exception_debug;
            }
        }
        
        /// <summary>
        /// 这个方法当一个异步发送操作完成时被调用.         
        /// </summary>
        /// <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            //SocketAsyncEventArgs token;
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            // 增加发送计数器
            Interlocked.Add(ref m_totalBytesWrite, e.BytesTransferred);

            if (e.Count > m_BufferSize)
            {
                lock (m_bufferLock)
                {
                    m_bufferManager.SetBuffer(e);// 恢复默认大小缓冲区
                }
                //e.SetBuffer(null, 0, 0);// 清除发送缓冲区
            }
            lock (m_writePool)
            {
                // 回收SocketAsyncEventArgs以备再次被利用
                m_writePool.Push(e);
            }
            // 清除UserToken对象引用  
            e.UserToken = null;

            if (e.SocketError == SocketError.Success)
            {
                Debug.WriteLine(string.Format("发送 字节数:{0}", e.BytesTransferred.ToString()));
                //lock (((ICollection)this.m_tokens).SyncRoot)
                //{
                //    if (!this.m_tokens.TryGetValue(token.ConnectionId, out token))
                //    {
                //        RaiseErrorEvent(token,new AsyncSocketException(string.Format("客户端:{0}或者已经关闭或者未连接", token.ConnectionId), AsyncSocketErrorCode.ClientSocketNoExist));
                //        //throw new AsyncSocketException(string.Format("客户端:{0}或者已经关闭或者未连接", token.ConnectionId), AsyncSocketErrorCode.ClientSocketNoExist);
                //        //return;
                //    }
                //}

                EventHandler<AsyncUserToken> handler = OnDataSendCompleted;
                // 如果订户事件将为空(null)
                if (handler != null)
                {
                    handler(this, token);//抛出客户端发送完成事件
                }

                //try
                //{
                //    // 读取下一块从客户端发送来的数据
                //    bool willRaiseEvent = ((AsyncUserToken)connection.UserToken).Socket.ReceiveAsync(connection);
                //    if (!willRaiseEvent)
                //    {
                //        ProcessReceive(connection);
                //    }
                //}
                //catch (ObjectDisposedException)
                //{
                //    RaiseDisconnectedEvent(connection);
                //}
                //catch (SocketException exception)
                //{
                //    if (exception.ErrorCode == (int)SocketError.ConnectionReset)//10054一个建立的连接被远程主机强行关闭
                //    {
                //        RaiseDisconnectedEvent(connection);//引发断开连接事件
                //    }
                //    else
                //    {
                //        RaiseErrorEvent(connection, exception);//引发断开连接事件
                //    }
                //}
                //catch (Exception exception_debug)
                //{
                //    Debug.WriteLine("调试：" + exception_debug.Message);
                //    throw exception_debug;
                //}
            }
            else
            {
                //lock (((ICollection)this.m_tokens).SyncRoot)
                //{
                //    if (!this.m_tokens.TryGetValue(token.ConnectionId, out token))
                //    {
                //        throw new AsyncSocketException(string.Format("客户端:{0}或者已经关闭或者未连接", token.ConnectionId), AsyncSocketErrorCode.ClientSocketNoExist);
                //        //return;
                //    }
                //}

                RaiseDisconnectedEvent(token);//引发断开连接事件
            }
        }
        
        public void Disconnect(string connectionId)//断开连接(形参 连接ID)
        {
            AsyncUserToken token;

            lock (((ICollection)this.m_tokens).SyncRoot)
            {
                if (!this.m_tokens.TryGetValue(connectionId, out token))
                {
                    throw new AsyncSocketException(string.Format("客户端:{0}已经关闭或者未连接", connectionId), AsyncSocketErrorCode.ClientSocketNoExist);
                    //return;//不存在该ID客户端
                }
            }

            RaiseDisconnectedEvent(token);//抛出断开连接事件            
        }
        
        private void RaiseDisconnectedEvent(AsyncUserToken token)//引发断开连接事件
        {
            if (null != token)
            {
                lock (((ICollection)this.m_tokens).SyncRoot)
                {
                    if (this.m_tokens.ContainsValue(token))
                    {
                        this.m_tokens.Remove(token.ConnectionId);
                        CloseClientSocket(token);
                        EventHandler<AsyncUserToken> handler = OnClientDisconnect;
                        // 如果订户事件将为空(null)
                        if ((handler != null) && (null != token))
                        {
                            handler(this, token);//抛出连接断开事件
                        }
                    }
                }
            }
        }

        private void CloseClientSocket(AsyncUserToken token)
        {
            //AsyncUserToken token = e.UserToken as AsyncUserToken;

            // 关闭相关联的客户端
            try
            {
                token.Socket.Shutdown(SocketShutdown.Both);
                token.Socket.Close();
            }
            // 抛出客户处理已经被关闭
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
                token.Socket.Close();
            }
            catch (Exception exception_debug)
            {
                token.Socket.Close();
                Debug.WriteLine("调试:" + exception_debug.Message);
                throw exception_debug;
            }
            finally
            {
                // 减少连接到服务器客户端总数的计数器的值
                Interlocked.Decrement(ref m_numConnectedSockets);
                m_maxNumberAcceptedClients.Release();

                Debug.WriteLine(string.Format("一个客户端被从服务器断开. 有 {0} 个客户端连接到服务器", m_numConnectedSockets.ToString()));
                lock (m_readPool)
                {
                    // 释放以使它们可以被其他客户端重新利用
                    m_readPool.Push(token.ReadEventArgs);
                }
            }

        }
        
        private void RaiseErrorEvent(AsyncUserToken token, AsyncSocketException exception)
        {
            EventHandler<AsyncSocketErrorEventArgs> handler = OnClientError;
            // 如果订户事件将为空(null)
            if (handler != null)
            {
                if (null != token)
                {
                    handler(token, new AsyncSocketErrorEventArgs(exception));//抛出客户端错误事件
                }
                else
                {
                    handler(null, new AsyncSocketErrorEventArgs(exception));//抛出服务器错误事件
                }
            }
        }

        public void Shutdown()
        {
            if (null != this.listenSocket)
            {
                this.listenSocket.Close();//停止侦听
            }
            lock (((ICollection)this.m_tokens).SyncRoot)
            {
                foreach (AsyncUserToken token in this.m_tokens.Values)
                {
                    try
                    {
                        CloseClientSocket(token);

                        EventHandler<AsyncUserToken> handler = OnClientDisconnect;
                        // 如果订户事件将为空(null)
                        if ((handler != null) && (null != token))
                        {
                            handler(this, token);//抛出连接断开事件
                        }
                    }
                    // 编译时打开注释调试时关闭
                    //catch(Exception){ }

                    // 编译时关闭调试时打开
                    catch (Exception exception_debug)
                    {
                        Debug.WriteLine("调试:" + exception_debug.Message);
                        throw exception_debug;
                    }
                }
                this.m_tokens.Clear();
            }
            isActive = false;
        }
    }
}