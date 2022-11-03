using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace VoiceChat
{
    public class TCPNetworkRoom : MonoBehaviour
    {
        private TCPNetworkListener _listener = new TCPNetworkListener();
        private int _playerIndex = 0;

        private ConcurrentQueue<ArraySegment<byte>> _queue = new ConcurrentQueue<ArraySegment<byte>>();
        private int _isAlbleQueue = 0;
        private int INT_ENABLE_CHANGE = 0;
        private int INT_UNABLE_CHANGE = 1;

        public void AddPlayer(TCPSession tcpSession)
        {
            //플레이어 +1하고 플레이어에게 ID값 보내기
            _playerIndex += 1;

            //TODO : TEST
            Debug.Log("=== 테스트 시작 1 ===");
            tcpSession.SendPacket((int)VoicePacketType.ACCEPT, BitConverter.GetBytes(84654641));

            //tcpSession.SendPacket((int)VoicePacketType.ACCEPT, BitConverter.GetBytes(_playerIndex));
            //Debug.Log("VoiceChat :: 플레이어가 접속되었습니다 :: " + _playerIndex);
        }


        public void StartTCPListener(int port, UnityAction ServerStartedEvent)
        {
            _listener.StartTcpListener(port, ServerStartedEvent, AddPlayer, OnReceivedPacket);
        }

        public void StopTCPListener()
        {
            _listener.StopTCPListener();
        }

        private void OnReceivedPacket(ArraySegment<byte> packet)
        {
            Debug.Log("Server : OnReceivedPacket");
            while (Interlocked.CompareExchange(ref _isAlbleQueue, INT_UNABLE_CHANGE, INT_ENABLE_CHANGE) == INT_ENABLE_CHANGE)
            {
            }
            _queue.Enqueue(packet);
            Interlocked.Exchange(ref _isAlbleQueue, INT_ENABLE_CHANGE);
        }

        private void Update()
        {
            if (_queue.Count > 0)
            {
                if (Interlocked.CompareExchange(ref _isAlbleQueue, INT_UNABLE_CHANGE, INT_ENABLE_CHANGE) == INT_UNABLE_CHANGE)
                {
                    ArraySegment<byte> result;
                    while (_queue.TryDequeue(out result))
                    {
                        Debug.Log("Server : _queue");
                        _listener.SendAllClients(result);
                        Interlocked.Exchange(ref _isAlbleQueue, INT_ENABLE_CHANGE);
                    }
                }
            }
        }
    }

}
