using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoiceChat
{
    public class VoicePlayer : MonoBehaviour
    {
        private int _playerId = 0;
        public int GetPlayerId(){ return _playerId; }
        [SerializeField]
        private TCPNetworkClient _networkClient;
        public TCPNetworkClient NetworkClient { get => _networkClient; }

        [SerializeField]
        private MicrophoneCapture _micCapture;
        [SerializeField]
        private MicrophoneDataSet _micDataSet;
        public void InitVoicePlayer(int playerId)
        {
            _playerId = playerId;
        }
        public void SetChangeMicName(string micName)
        {
            _micCapture.SetChangeMicName(micName);
        }

        public void StartSendVoicePacket()
        {
            _micCapture.StartCapture(SendVoicePacket);
        }

        private void SendVoicePacket(VoiceData voiceData)
        {
            _networkClient.SendPacket(new VoiceChatPacket(_playerId, VoicePacketType.VOICE, 0, voiceData));
        }

        public void InitElsePlayerVoice()
        {
            _micDataSet.StartVoiceClient(_playerId);
        }

        public void SetVoicePacketData(VoiceData voiceData)
        {
            _micDataSet.SetVoiceData(voiceData.voiceArray, voiceData.voiceID, voiceData.voiceIndex);
        }
        
        public void StopVoicePlayer()
        {
            _networkClient.DisconnectClient();
            _micCapture.StopCapture();
        }
    }

}
