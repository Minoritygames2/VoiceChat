using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;

namespace VoiceChat
{
    public class UDPNetworkSender
    {
        /// <summary>
        /// 수신처
        /// </summary>
        private UdpClient _udpClient;
        private IPEndPoint _endPoint;

        private bool _isStarted = false;
        /// <summary>
        /// UDP 수신처 초기화
        /// </summary>
        public void ConnectBroadcastUDPClient(int port)
        {
            if (_udpClient != null)
                _udpClient.Close();

            _isStarted = true;

            _udpClient = new UdpClient();
            _endPoint = new IPEndPoint(IPAddress.Broadcast, port);
            _udpClient.EnableBroadcast = true;
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.ExclusiveAddressUse = false;
            _udpClient.Connect(_endPoint);

            Debug.Log("Network Sender 접속 : " + _endPoint.Address.ToString() + " : " + _endPoint.Port.ToString());
        }

        public void StopUDPSender()
        {
            if(_isStarted)
                _udpClient.Close();

            _isStarted = false;
        }

        /// <summary>
        /// UDP 송신
        /// </summary>
        /// <param name="packet">송신 패킷</param>
        public void SendPacket(byte[] packet)
        {
            try
            {
                if (!_isStarted)
                    return;
                _udpClient.BeginSend(packet, packet.Length, new AsyncCallback(SendCallback), _udpClient);
            }
            catch(Exception e)
            {
                throw (new Exception("VoiceChat :: 네트워크 ::송신에러 " + e.Message));
            }
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            if (!_isStarted)
                return;
            UdpClient udpClient = (UdpClient)asyncResult.AsyncState;
            udpClient.EndSend(asyncResult);
        }
    }

}
