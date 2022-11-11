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

        private CanvasPlayerItem _canvasItem;
        public void InitVoicePlayer(int playerId, CanvasPlayerItem canvasItem)
        {
            _playerId = playerId;
            _canvasItem = canvasItem;
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
        
        public void SendDisconnectRequest()
        {
            //상태를 Disconnect로 바꾸기 => 다른 Send를 하지않기위해 
            _networkClient.RequestDisconnect();
            //Disconnect 보내기
            _networkClient.SendPacket(new VoiceChatPacket(_playerId, VoicePacketType.DISCONNECT_REQUEST, 0, new byte[0]));
        }
        public void StopVoicePlayer()
        {
            //접속끊기
            _networkClient.SessionClose();
            _micCapture.StopCapture();
        }

        public void KickPlayer()
        {
            _networkClient.SessionClose();
            Destroy(_canvasItem.gameObject);
        }
    }

}
