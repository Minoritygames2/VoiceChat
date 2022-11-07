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
        protected override void OnReceivedPacket(VoiceChatPacket voicePacket)
        {
            if (voicePacket.playerId == 0)
                ServerPacket((int)voicePacket.packetType, voicePacket.message);
        }

        public void StartTcpClient()
        {

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

        private void ClientPacket(int playerId, int packetType, byte[] message)
        {
            if (_myVoicePlayer.GetPlayerId() == playerId)
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
            int size = 0;
            var voiceId = BitConverter.ToInt32(message, size);
            size += 4;

            var voiceIndex = BitConverter.ToInt32(message, size);
            size += 4;

            var timeSamples = BitConverter.ToInt32(message, size);
            size += 4;

            var voicePacket = new byte[message.Length - size];
            Buffer.BlockCopy(message, size, voicePacket, 0, message.Length - size);
        }

        /// <summary>
        /// voice Packet
        /// </summary>
        public void SendPacket(VoiceData voiceData)
        {
            int nowPosition = 0;
            var sendBuffer = new TCPSendBuffer(4096 * 100);
            var voiceBuffer = sendBuffer.OpenBuffer();
            var voiceIdByte = BitConverter.GetBytes(voiceData.voiceID);
            Buffer.BlockCopy(voiceIdByte, 0, voiceBuffer.Array, nowPosition, voiceIdByte.Length);
            nowPosition += voiceIdByte.Length;

            var voiceIndex = BitConverter.GetBytes(voiceData.voiceIndex);
            Buffer.BlockCopy(voiceIndex, 0, voiceBuffer.Array, nowPosition, voiceIndex.Length);
            nowPosition += voiceIndex.Length;

            Buffer.BlockCopy(voiceData.voiceArray, 0, voiceBuffer.Array, nowPosition, voiceData.voiceArray.Length);
            nowPosition += voiceData.voiceArray.Length;

            var rsltCloseBuffer = sendBuffer.CloseBuffer(nowPosition);
            Buffer.BlockCopy(voiceBuffer.Array, 0, rsltCloseBuffer.Array, 0, rsltCloseBuffer.Count);
        }

    }
}

