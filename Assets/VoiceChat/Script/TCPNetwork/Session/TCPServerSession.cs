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

        protected override void OnReceivedPacket(VoiceChatPacket voicePacket)
        {

        }

        public void AddPlayer(TcpClient tcpClient)
        {

        }
    }
}

