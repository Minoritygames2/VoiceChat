using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace VoiceChat
{
    public class ServerNetworkController : MonoBehaviour
    {
        [SerializeField]
        private TCPNetworkListener _tcpListener;
        private UDPNetworkSender _udpSender = new UDPNetworkSender();

        /// <summary>
        /// 서버 기동
        /// </summary>
        public void StartServer(string portStr, UnityAction OnStartedServer)
        {
            int portNo = 0;
            if (!int.TryParse(portStr, out portNo))
            {
                Debug.Log("VoiceChat :: 포트번호가 옳지않습니다");
                return;
            }

            _tcpListener.StartTCPListener(portNo, OnStartedServer);
        }

        /// <summary>
        /// 서버 서치허용
        /// </summary>
        public void StartDetectedServer(string portStr)
        {
            StopAllCoroutines();

            int portNo = 0;
            if (!int.TryParse(portStr, out portNo))
            {
                Debug.Log("포트번호가 옳지않습니다");
                return;
            }
            _udpSender.ConnectBroadcastUDPClient(portNo);
        }

        public string GetLocalIPv4()
        {
            return Dns.GetHostEntry(Dns.GetHostName())
                .AddressList.First(
                    f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .ToString();
        }

        public void SendUDPDetectPacket()
        {
            _udpSender.SendPacket(Encoding.UTF8.GetBytes(PacketJsonConverter.JsonToString(new ServerDetected() { ServerIP = GetLocalIPv4() })));
        }

        public void StopUDPPacket()
        {
            _udpSender.StopUDPSender();
        }

        private void OnDestroy()
        {
            _tcpListener.StopTCPListener();
                _udpSender.StopUDPSender();
        }
    }

}
