using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace VoiceChat
{
    public class TCPNetworkClient : MonoBehaviour
    {
        #region Client Status
        private struct ClientStatus
        {
            public int _isConnected;
        }
        private ClientStatus _myStatus;
        private TcpClient _tcpClient;
        // 0 : 비접속 / 1 : 접속중
        private int _isConnected = 0;
        #endregion

        public UnityEvent OnClientConnected = new UnityEvent();
        public UnityEvent OnClientDisconnected = new UnityEvent();
        public VoiceEvent OnReceiveVoicePacket = new VoiceEvent();
        private TCPSession _tcpSession;

        private ConcurrentQueue<ArraySegment<byte>> _queue = new ConcurrentQueue<ArraySegment<byte>>();
        private int _isAlbleQueue = 0;
        private int INT_ENABLE_CHANGE = 0;
        private int INT_UNABLE_CHANGE = 1;

        #region 접속/송수신 모듈
        /// <summary>
        /// TCP클라이언트 접속
        /// </summary>
        public void StartTcpClient(string ipStr)
        {
            if (_myStatus._isConnected == 1)
            {
                Debug.Log("VoiceChat :: 이미 서버에 접속중입니다");
                return;
            }
            try
            {
                _tcpClient = new TcpClient();
                _tcpClient.BeginConnect(IPAddress.Parse(ipStr), 5555, new AsyncCallback(ConnectCallback), _tcpClient.Client);
            }
            catch (Exception e)
            {
                Debug.Log("VoiceChat :: 네트워크 :: 클라이언트 접속 실패 : " + e.Message);
            }
        }

        private void ConnectCallback(IAsyncResult asyncResult)
        {
            _isConnected = 1;
            _tcpSession = new TCPSession((Socket)asyncResult.AsyncState, OnPacketSended, OnPacketReceived);
        }

        /// <summary>
        /// 클라이언트 접속끊기
        /// </summary>
        public void DisconnectClient()
        {
            if (_tcpSession != null)
                _tcpSession.SessionClose();
            _isConnected = 0;
        }

        /// <summary>
        /// 클라이언트 패킷송신
        /// </summary>
        public void SendPacket(byte[] buffer)
        {
            _tcpSession.SendPacket(0, buffer);
        }

        public void SendVoicePacket(VoiceData voiceData)
        {
            _tcpSession.SendPacket(voiceData);
        }
        #endregion

        private void OnPacketSended(ArraySegment<byte> packet){}
        private void OnPacketReceived(ArraySegment<byte> packet)
        {
            while (Interlocked.CompareExchange(ref _isAlbleQueue, INT_UNABLE_CHANGE, INT_ENABLE_CHANGE) == INT_ENABLE_CHANGE)
            {
            }
            _queue.Enqueue(packet);
            Interlocked.Exchange(ref _isAlbleQueue, INT_ENABLE_CHANGE);
        }

        #region 통신 이벤트 처리
        private void Update()
        {
            //클라이언트 접속확인
            if (_myStatus._isConnected != _isConnected)
                ChangeConnectedStatus();

            if (_queue.Count > 0)
            {
                if (Interlocked.CompareExchange(ref _isAlbleQueue, INT_UNABLE_CHANGE, INT_ENABLE_CHANGE) == INT_UNABLE_CHANGE)
                {
                    ArraySegment<byte> result;
                    while (_queue.TryDequeue(out result))
                    {
                        CheckPacket(result.Array);
                        Interlocked.Exchange(ref _isAlbleQueue, INT_ENABLE_CHANGE);
                    }
                }
            }
        }

        /// <summary>
        /// 접속 정보 변경
        /// </summary>
        public void ChangeConnectedStatus()
        {
            if (_isConnected == 0)
            {
                Debug.Log("VoiceChat :: 네트워크 :: 클라이언트 접속종료 ");
                OnClientDisconnected?.Invoke();
            }
            else if (_isConnected == 1)
            {
                Debug.Log("VoiceChat :: 네트워크 :: 클라이언트 접속 ");
                OnClientConnected?.Invoke();
            }

            _myStatus._isConnected = _isConnected;
        }

        private void CheckPacket(byte[] packet)
        {
            int size = 0;
            var playerId = BitConverter.ToInt32(packet, size);
            size += 4;
            var packetType = BitConverter.ToInt32(packet, size);
            size += 4;
            var channel = BitConverter.ToInt32(packet, size);
            size += 4;
            var message = new byte[packet.Length - size];
            Buffer.BlockCopy(packet, size, message, 0,  packet.Length - size);

            if (playerId == 0)
                ServerPacket(playerId, message);
            else
                ClientPacket(playerId, packetType, message);
        }

        private void ServerPacket(int packetType, byte[] message)
        {
            Debug.Log("Message : " + ((VoicePacketType)packetType).ToString());
            switch((VoicePacketType)packetType)
            {
                case VoicePacketType.ACCEPT:
                    var playerId = BitConverter.ToInt32(message, 0);
                    Debug.Log("voiceChat :: 플레이어 등록 " + playerId);
                    _tcpSession.VoiceClientStatus.PlayerId = playerId;
                    break;
                default:
                    break;
            }
        }

        private void ClientPacket(int playerId, int packetType, byte[] message)
        {
            if (_tcpSession.VoiceClientStatus.PlayerId == playerId)
                return;

            switch ((VoicePacketType)packetType)
            {
                case VoicePacketType.VOICE:
                    ParseVoicePacket(playerId, message);
                    break;
                default:
                    break;
            }
        }

        private void ParseVoicePacket(int playerId, byte[] message)
        {
            ArraySegment<byte> packet = new ArraySegment<byte>(message,0,message.Length);

            int size = 0;
            var voiceId = BitConverter.ToInt32(message, size);
            size += 4;

            var voiceIndex = BitConverter.ToInt32(message, size);
            size += 4;
            var voicePacket = new byte[message.Length - size];
            Buffer.BlockCopy(message, size, voicePacket, 0, message.Length - size);

            OnReceiveVoicePacket?.Invoke(new VoiceData() { networkId = playerId, voiceID = voiceId, voiceIndex = voiceIndex, voiceArray = voicePacket });
        }
        #endregion

        private void OnDestroy()
        {
            DisconnectClient();
        }
    }

}
