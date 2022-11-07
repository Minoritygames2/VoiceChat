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

        }

        public void AddPlayer(TcpClient tcpClient)
        {
            _index++;
            var newClient = AddTcpClient(tcpClient);
            _voicePlayer.Add(newClient);
            newClient.InitVoicePlayer(_index);
            newClient.NetworkClient.SendPacket(new VoiceChatPacket(0, VoicePacketType.ACCEPT, 0, new byte[0]));
        }
    }
}

