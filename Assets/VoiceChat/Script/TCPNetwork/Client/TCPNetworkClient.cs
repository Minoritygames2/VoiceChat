using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace VoiceChat
{
    public class TCPNetworkClient : MonoBehaviour
    {
        private TcpClient _tcpClient = new TcpClient();
        [SerializeField]
        private TCPClientSession _clientSession;
        
        private int _isConnected = 0;
        #region 접속/송수신 모듈
        /// <summary>
        /// TCP클라이언트 접속
        /// </summary>
        public void StartTcpClient(string ipStr, UnityAction OnClientConnected, UnityAction<VoiceData> OnReceiveVoicePacket)
        {
            if (_isConnected != 0)
            {
                Debug.Log("VoiceChat :: 이미 서버에 접속중입니다");
                return;
            }

            _clientSession.OnClientConnected.AddListener(OnClientConnected);
            _clientSession.OnReceiveVoicePacket.AddListener(OnReceiveVoicePacket);

            try
            {
                _isConnected = 1;
                _tcpClient = new TcpClient();
                _tcpClient.BeginConnect(IPAddress.Parse(ipStr), 5555, new AsyncCallback(ConnectCallback), _tcpClient.Client);
            }
            catch (Exception e)
            {
                Debug.Log("VoiceChat :: 네트워크 :: 클라이언트 접속 실패 : " + e.Message);
            }
        }

        private void ConnectCallback(IAsyncResult asyncResult)
        {
            _isConnected = 2;
            _clientSession.OnConnectClient((Socket)asyncResult.AsyncState);
        }

        /// <summary>
        /// 클라이언트 접속끊기
        /// </summary>
        public void DisconnectClient()
        {
            _isConnected = 0;
        }

        #endregion

        #region 통신 이벤트 처리

        public void SendVoicePacket(VoiceData voiceData)
        {
            if (_isConnected != 0)
                _clientSession.SendPacket(voiceData);
        }
        #endregion

        private void OnDestroy()
        {
            DisconnectClient();
        }
    }

}
