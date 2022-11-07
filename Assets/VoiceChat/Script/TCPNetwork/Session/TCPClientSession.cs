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
    public class TCPClientSession : TCPSession
    {
        private VoicePlayer _myVoicePlayer;
        private List<VoicePlayer> _players = new List<VoicePlayer>();
        protected override void OnReceivedPacket(VoiceChatPacket voicePacket)
        {
            if (voicePacket.playerId == 0)
                ServerPacket((int)voicePacket.packetType, voicePacket.message);
        }

        public void StartTcpClient(string serverIP, string micName)
        {
            _myVoicePlayer = AddTcpClient(serverIP, ()=>{ _myVoicePlayer.StartSendVoicePacket(micName); }, OnReceiveVoicePacket);
        }

        private void OnReceiveVoicePacket(VoiceChatPacket voicePacket)
        {
            if (voicePacket.playerId == _myVoicePlayer.GetPlayerId())
                return;
            else if (voicePacket.playerId == 0)
                ServerPacket((int)voicePacket.packetType, voicePacket.message);
            else
                ClientPacket(voicePacket.playerId, (int)voicePacket.packetType, voicePacket.message, voicePacket.voiceData);
        }
        private void ServerPacket(int packetType, byte[] message)
        {
            switch ((VoicePacketType)packetType)
            {
                case VoicePacketType.ACCEPT:
                    var playerId = BitConverter.ToInt32(message, 0);
                    _myVoicePlayer.InitVoicePlayer(playerId);
                    break;
                default:
                    break;
            }
        }

        private void ClientPacket(int playerId, int packetType, byte[] message, VoiceData voicdData)
        {
            switch ((VoicePacketType)packetType)
            {
                case VoicePacketType.VOICE:
                    break;
                default:
                    break;
            }
        }


    }
}

