using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace VoiceChat
{
    public abstract class TCPSession : MonoBehaviour
    {
        protected List<TCPNetworkClient> _tcpClient = new List<TCPNetworkClient>();
        private VoiceClientStatus _voiceClientStatus = new VoiceClientStatus();
        public VoiceClientStatus VoiceClientStatus { get => _voiceClientStatus; set => _voiceClientStatus = value; }

        protected abstract void OnReceivedPacket(VoiceChatPacket voicePacket);

        public void AddTcpClient(TCPNetworkClient tcpClient)
        {
            tcpClient.OnReceiveVoicePacket.AddListener(OnReceivedPacket);
            _tcpClient.Add(tcpClient);
        }
    }
}
