using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace VoiceChat
{
    public class TCPServerSession : MonoBehaviour
    {
        public delegate void TCPSessionEvent(TCPSession tcpSession);

        #region ROOM
        private int _playerIndex = 0;
        private List<TCPNetworkClient> _tcpClients = new List<TCPNetworkClient>();
        public class TCPNetworkClient
        {
            private int _playerIndex = 0;
            private TCPSession _tcpSesstion;
            public TCPSession Client { get => _tcpSesstion; }
            public TCPNetworkClient(int playerIndex, TCPSession clientSession)
            {
                _playerIndex = playerIndex;
                _tcpSesstion = clientSession;
            }
        }
        public void AddPlayer(TcpClient tcpClient)
        {
            _playerIndex++;
            var tcpSession = new TCPSession(tcpClient.Client, OnSendPacket, OnReceivedPacket);
            _tcpClients.Add(new TCPNetworkClient(_playerIndex, tcpSession));
            tcpSession.SendPacket((int)VoicePacketType.ACCEPT, BitConverter.GetBytes(_playerIndex));
            Debug.Log("VoiceChat :: 플레이어가 접속되었습니다 :: " + _playerIndex);
        }
        #endregion

        private struct SendPacketStruct
        {
            public ArraySegment<byte> sendPacket;
            public TCPSession sendSession;
        }

        private ConcurrentQueue<SendPacketStruct> _sendQueue = new ConcurrentQueue<SendPacketStruct>();
        private int _isAlbleSendQueue = 0;
        private int INT_ENABLE_SEND = 0;
        private int INT_UNABLE_SEND = 1;

        private ConcurrentQueue<ArraySegment<byte>> _receiveQueue = new ConcurrentQueue<ArraySegment<byte>>();
        private int _isAlbleReceiveQueue = 0;
        private int INT_ENABLE_RECEIVE = 0;
        private int INT_UNABLE_RECEIVE = 1;


        private void OnSendPacket(ArraySegment<byte> packet)
        {
            Interlocked.Exchange(ref _isAlbleReceiveQueue, INT_ENABLE_RECEIVE);
        }

        private void OnReceivedPacket(ArraySegment<byte> packet)
        {
            _receiveQueue.Enqueue(packet);
        }

        private void Update()
        {
            if (_receiveQueue.Count > 0)
            {
                if (Interlocked.CompareExchange(ref _isAlbleReceiveQueue, INT_UNABLE_RECEIVE, INT_ENABLE_RECEIVE) == INT_UNABLE_RECEIVE)
                {
                    ArraySegment<byte> result;
                    while (_receiveQueue.TryDequeue(out result))
                    {
                        SendAllClients(result);
                        Interlocked.Exchange(ref _isAlbleReceiveQueue, INT_ENABLE_RECEIVE);
                    }
                }
            }

            if (_sendQueue.Count > 0)
            {
                if (Interlocked.CompareExchange(ref _isAlbleSendQueue, INT_UNABLE_SEND, INT_ENABLE_SEND) == INT_UNABLE_SEND)
                {
                    SendPacketStruct result;
                    while (_sendQueue.TryDequeue(out result))
                    {
                        result.sendSession.SendPacket(result.sendPacket);
                    }
                }
            }
        }

        public void SendPacket(ArraySegment<byte> packet)
        {
            _receiveQueue.Enqueue(packet);
        }

        public void SendAllClients(ArraySegment<byte> packet)
        {
            for (int index = 0; index < _tcpClients.Count; index++)
                if (_tcpClients[index].Client.IsConnected())
                    _tcpClients[index].Client.SendPacket(packet);
        }
    }
}

