using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace VoiceChat
{
    public class TCPServerSession : TCPSession
    {
        public delegate void TCPSessionEvent(TCPSession tcpSession);

        private List<VoicePlayer> _voicePlayer = new List<VoicePlayer>();
        private int _index = 0;

        [SerializeField]
        private ServerCanvasController _canvasController;

        private ConcurrentQueue<VoicePlayer> _disConnectReqQueue = new ConcurrentQueue<VoicePlayer>();
        private int _isAlbleDisConnectQueue = 0;
        private int INT_ENABLE_DISCONNECT = 0;
        private int INT_UNABLE_DISCONNECT = 1;
        protected override void OnReceivedPacket(VoiceChatPacket voicePacket)
        {
            switch(voicePacket.packetType)
            {
                case VoicePacketType.UNKNOWN:
                    break;
                case VoicePacketType.ACCEPT:
                    break;
                case VoicePacketType.CONNECT_REQUEST:
                    break;
                case VoicePacketType.CONNECT_RESPONCE:
                    break;
                case VoicePacketType.DISCONNECT_REQUEST:
                    DisconnectResponse(voicePacket.playerId);
                    break;
                case VoicePacketType.DISCONNECT_RESPONCE:
                    break;
                case VoicePacketType.MESSAGE:
                    break;
                case VoicePacketType.VOICE:
                    SendPacketToClient(voicePacket);
                    break;
            }
        }

        public void AddPlayer(TcpClient tcpClient)
        {
            _index++;
            var newClient = AddTcpClient(tcpClient);
            _voicePlayer.Add(newClient);
            newClient.InitVoicePlayer(_index, _canvasController.CreateCanvasPlayerItem(_index));
            newClient.NetworkClient.SendPacket(new VoiceChatPacket(0, VoicePacketType.ACCEPT, 0, BitConverter.GetBytes(_index)));
            _canvasController.ChangePlayerCount(_voicePlayer.Count);
        }

        /// <summary>
        /// 클라이언트 접속종료
        /// </summary>
        private void DisconnectResponse(int playerId)
        {
            if (AnyClient(playerId))
                SendDisconnectResponseToClient(playerId);
        }

        private void SendDisconnectResponseToClient(int playerId)
        {
            var disconnectClient = WhereClient(playerId);
            var playerIdByte = BitConverter.GetBytes(playerId);
            for (int index = 0; index < _voicePlayer.Count; index++)
            {
                if (_voicePlayer[index].GetPlayerId() == playerId)
                {
                    _voicePlayer[index].NetworkClient.SendPacket(
                           new VoiceChatPacket(0, VoicePacketType.DISCONNECT_RESPONCE, 0, playerIdByte)
                           , () => { _voicePlayer.Remove(disconnectClient); });
                    _disConnectReqQueue.Enqueue(_voicePlayer[index]);
                    _voicePlayer[index].NetworkClient.RequestDisconnect();
                }
                else
                    _voicePlayer[index].NetworkClient.SendPacket(new VoiceChatPacket(0, VoicePacketType.DISCONNECT_RESPONCE, 0, playerIdByte));
            }
        }

        /// <summary>
        /// 클라이언트들에게 패킷을 보낸다
        /// </summary>
        /// <param name="voicePacket">보낼패킷</param>
        private void SendPacketToClient(VoiceChatPacket voicePacket)
        {
            for(int index = 0; index < _voicePlayer.Count; index++)
            {
                if (_voicePlayer[index].GetPlayerId() == voicePacket.playerId)
                    continue;
                _voicePlayer[index].NetworkClient.SendPacket(voicePacket);
            }
        }

        private bool AnyClient(int playerId)
        {
            for (int index = 0; index < _voicePlayer.Count; index++)
                if (_voicePlayer[index].GetPlayerId() == playerId)
                    return true;
            return false;
        }

        private VoicePlayer WhereClient(int playerId)
        {
            for (int index = 0; index < _voicePlayer.Count; index++)
                if (_voicePlayer[index].GetPlayerId() == playerId)
                    return _voicePlayer[index];
            return null;
        }

        private void Update()
        {
            if (_disConnectReqQueue.Count > 0)
            {
                if (Interlocked.CompareExchange(ref _isAlbleDisConnectQueue, INT_UNABLE_DISCONNECT, INT_ENABLE_DISCONNECT) == INT_UNABLE_DISCONNECT)
                {
                    VoicePlayer removePlayer;
                    while (_disConnectReqQueue.Count > 0)
                    {
                        while (_disConnectReqQueue.TryDequeue(out removePlayer))
                        {
                            _voicePlayer.Remove(removePlayer);
                            removePlayer.KickPlayer();
                            Destroy(removePlayer.gameObject);
                        }
                    }
                    Interlocked.Exchange(ref _isAlbleDisConnectQueue, INT_ENABLE_DISCONNECT);
                    _canvasController.ChangePlayerCount(_voicePlayer.Count);
                }
            }
        }
    }
} 

