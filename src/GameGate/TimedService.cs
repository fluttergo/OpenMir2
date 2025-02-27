﻿using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using SystemModule;
using SystemModule.Packages;

namespace GameGate
{
    public class TimedService : BackgroundService
    {
        private LogQueue _logQueue => LogQueue.Instance;
        private ClientManager _clientManager => ClientManager.Instance;
        private SessionManager _sessionManager => SessionManager.Instance;
        private ServerManager _serverManager => ServerManager.Instance;

        private int _processDelayTick = 0;
        private int _processDelayCloseTick = 0;
        private int _processClearSessionTick = 0;
        private int _kepAliveTick = 0;

        public TimedService()
        {
            _kepAliveTick = HUtil32.GetTickCount();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                OutMianMessage();
                ProcessDelayMsg();
                ClearSession();
                KeepAlive();
                await Task.Delay(TimeSpan.FromMilliseconds(10), stoppingToken);
            }
        }

        private void OutMianMessage()
        {
            if (GateShare.ShowLog)
            {
                while (!_logQueue.MessageLog.IsEmpty)
                {
                    string message;
                    if (!_logQueue.MessageLog.TryDequeue(out message)) continue;
                    Console.WriteLine(message);
                }

                while (!_logQueue.DebugLog.IsEmpty)
                {
                    string message;
                    if (!_logQueue.DebugLog.TryDequeue(out message)) continue;
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.WriteLine(message);
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// GameGate->GameSvr 心跳
        /// </summary>
        private void KeepAlive()
        {
            if (HUtil32.GetTickCount() - _kepAliveTick > 10 * 10000)
            {
                _kepAliveTick = HUtil32.GetTickCount();
                var _serverList = _serverManager.GetServerList();
                for (int i = 0; i < _serverList.Count; i++)
                {
                    if (_serverList[i] == null)
                    {
                        continue;
                    }
                    if (_serverList[i].ClientThread == null)
                    {
                        continue;
                    }
                    if (!_serverList[i].ClientThread.IsConnected)
                    {
                        continue;
                    }
                    var cmdPacket = new PacketHeader();
                    cmdPacket.PacketCode = Grobal2.RUNGATECODE;
                    cmdPacket.Socket = 0;
                    cmdPacket.Ident = Grobal2.GM_CHECKCLIENT;
                    cmdPacket.PackLength = 0;
                    _serverList[i].ClientThread.SendBuffer(cmdPacket.GetBuffer());
                }
            }
        }

        /// <summary>
        /// 处理网关延时消息
        /// </summary>
        private void ProcessDelayMsg()
        {
            if (HUtil32.GetTickCount() - _processDelayTick > 100)
            {
                _processDelayTick = HUtil32.GetTickCount();
                var _serverList = _serverManager.GetServerList();
                for (var i = 0; i < _serverList.Count; i++)
                {
                    if (_serverList[i] == null)
                    {
                        continue;
                    }
                    if (HUtil32.GetTickCount() - _processDelayCloseTick > 2000) //加入网关延时发送关闭消息
                    {
                        _processDelayCloseTick = HUtil32.GetTickCount();
                        _serverList[i].ProcessCloseList();
                    }
                    if (_serverList[i].ClientThread == null)
                    {
                        continue;
                    }
                    if (_serverList[i].ClientThread.SessionArray == null)
                    {
                        continue;
                    }
                    for (var j = 0; j < _serverList[i].ClientThread.SessionArray.Length; j++)
                    {
                        var session = _serverList[i].ClientThread.SessionArray[j];
                        if (session?.Socket == null)
                        {
                            continue;
                        }
                        var userClient = _sessionManager.GetSession(session.SessionId);
                        userClient?.HandleDelayMsg();
                    }
                }
            }
        }

        /// <summary>
        /// 清理过期会话
        /// </summary>
        private void ClearSession()
        {
            if (HUtil32.GetTickCount() - _processClearSessionTick > 20000)
            {
                _processClearSessionTick = HUtil32.GetTickCount();
                _logQueue.EnqueueDebugging("清理超时会话开始工作...");
                var serverList = _serverManager.GetServerList();
                for (var i = 0; i < serverList.Count; i++)
                {
                    if (serverList[i] == null)
                    {
                        continue;
                    }
                    if (serverList[i].ClientThread == null)
                    {
                        continue;
                    }
                    ClientThread clientThread = serverList[i].ClientThread;
                    if (clientThread == null)
                    {
                        continue;
                    }
                    clientThread.CheckTimeOutSession();
                    _clientManager.CheckSessionStatus(serverList[i].ClientThread);
                }
                _logQueue.EnqueueDebugging("清理超时会话工作完成...");
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}