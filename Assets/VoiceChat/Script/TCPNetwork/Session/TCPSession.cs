using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace VoiceChat
{
    public abstract class TCPSession : MonoBehaviour
    {
        [SerializeField]
        private GameObject _playerPrefab;
        [SerializeField]
        private Transform _playerTransform;
        private VoiceClientStatus _voiceClientStatus = new VoiceClientStatus();
        public VoiceClientStatus VoiceClientStatus { get => _voiceClientStatus; set => _voiceClientStatus = value; }

        protected abstract void OnReceivedPacket(VoiceChatPacket voicePacket);

        public VoicePlayer AddTcpClient(TcpClient tcpClient)
        {
            var client = Instantiate(_playerPrefab, _playerTransform).GetComponent<VoicePlayer>();
            client.NetworkClient.OnReceiveVoicePacket.AddListener(OnReceivedPacket);
            return client;
        }
    }
}
