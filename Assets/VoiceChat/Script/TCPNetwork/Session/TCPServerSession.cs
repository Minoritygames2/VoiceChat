using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace VoiceChat
{
    public class TCPServerSession : TCPSession
    {
        public delegate void TCPSessionEvent(TCPSession tcpSession);

        private List<VoicePlayer> _voicePlayer = new List<VoicePlayer>();
        private int _index = 0;
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
                    RemovePlayer(voicePacket.playerId);
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
            newClient.InitVoicePlayer(_index);
            newClient.NetworkClient.SendPacket(new VoiceChatPacket(0, VoicePacketType.ACCEPT, 0, BitConverter.GetBytes(_index)));
        }

        /// <summary>
        /// 클라이언트 접속종료
        /// </summary>
        private void RemovePlayer(int playerId)
        {
            if (AnyClient(playerId))
            {
                var disconnectClient = WhereClient(playerId);
                _voicePlayer.Remove(disconnectClient);
                var playerIdByte = BitConverter.GetBytes(playerId);
                SendPacketToClient(new VoiceChatPacket(0, VoicePacketType.DISCONNECT_RESPONCE, 0, playerIdByte));

                Destroy(disconnectClient.gameObject);
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
    }
} 

