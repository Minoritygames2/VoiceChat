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
                ServerPacket(voicePacket);
        }

        public void StartTcpClient(string serverIP, string micName)
        {
            _myVoicePlayer = AddTcpClient(serverIP, ()=>{ _myVoicePlayer.SetChangeMicName(micName); }, OnReceiveVoicePacket);
        }
        
        public void StopTcpClient()
        {
            _myVoicePlayer.SendDisconnectRequest();
            _players.Clear();
        }

        private void OnReceiveVoicePacket(VoiceChatPacket voicePacket)
        {
            if (voicePacket.playerId == 0)
                ServerPacket(voicePacket);
            else
                ClientPacket(voicePacket);
        }
        private void ServerPacket(VoiceChatPacket voicePacket)
        {
            switch (voicePacket.packetType)
            {
                case VoicePacketType.ACCEPT:
                    _myVoicePlayer.InitVoicePlayer(BitConverter.ToInt32(voicePacket.message));
                    _myVoicePlayer.StartSendVoicePacket();
                    break;
                case VoicePacketType.DISCONNECT_RESPONCE:
                    if (voicePacket.playerId == _myVoicePlayer.GetPlayerId())
                        _myVoicePlayer.StopVoicePlayer();
                    else
                        RemoveVoiceClient(voicePacket.playerId);
                    break;
                default:
                    break;
            }
        }

        private void ClientPacket(VoiceChatPacket voicePacket)
        {
            if (voicePacket.playerId == _myVoicePlayer.GetPlayerId())
                return;
            switch (voicePacket.packetType)
            {
                case VoicePacketType.VOICE:
                    SetVoicePacket(voicePacket.voiceData);
                    break;
                default:
                    break;
            }
        }

        private void SetVoicePacket(VoiceData voicdData)
        {
            VoicePlayer client;
            if (ClientAny(voicdData.networkId))
                client = ClientWhere(voicdData.networkId);
            else
            {
                client = AddVoiceClient(voicdData.networkId);
                client.InitElsePlayerVoice();
            }
                

            client.SetVoicePacketData(voicdData);
        }

        #region ClientList
        public bool ClientAny(int playerId)
        {
            for(int index = 0; index < _players.Count; index ++)
                if (_players[index].GetPlayerId() == playerId)
                    return true;
            return false;
        }

        public VoicePlayer ClientWhere(int playerId)
        {
            for (int index = 0; index < _players.Count; index++)
                if (_players[index].GetPlayerId() == playerId)
                    return _players[index];
            return null;
        }

        public VoicePlayer AddVoiceClient(int playerId)
        {
            var newPlayer = AddTcpClient();
            newPlayer.InitVoicePlayer(playerId);
            _players.Add(newPlayer);
            return newPlayer;
        }

        public void RemoveVoiceClient(int playerId)
        {
            if(ClientAny(playerId))
            {
                var client = ClientWhere(playerId);
                _players.Remove(client);
                Destroy(client.gameObject);
                Debug.Log(string.Format("네트워크 :: {0}번 플레이어 로그아웃", playerId));
            }
        }
        #endregion
    }
}

