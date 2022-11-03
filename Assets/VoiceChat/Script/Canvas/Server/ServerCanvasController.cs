using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

namespace VoiceChat
{
    public class ServerCanvasController : MonoBehaviour
    {
        [SerializeField]
        private InputField _inputPort;
        [SerializeField]
        private Button _btnStartServer;
        [SerializeField]
        private Transform _startServer;
        [SerializeField]
        private Transform _startedServer;
        [SerializeField]
        private Button _btnDetectedServer;
        [SerializeField]
        private Text _txtDetectedTime;

        [SerializeField]
        private TCPNetworkRoom _tcpRoom;
        private UDPNetworkSender _udpSender = new UDPNetworkSender();

        //서버 찾기 대기시간
        private WaitForSeconds _wait = new WaitForSeconds(0.1f);
        private bool _isSearch = false;
        private int _searchWaitTime = 0;
        private void Start()
        {
            _btnStartServer.onClick.AddListener(StartServer);
            _btnDetectedServer.onClick.AddListener(StartDetectedServer);
        }

        /// <summary>
        /// 서버 기동
        /// </summary>
        public void StartServer()
        {
            if (_inputPort.text.Equals(string.Empty))
            {
                Debug.Log("VoiceChat :: 포트번호를 입력해주세요");
                return;
            }

            int portNo = 0;
            if (!int.TryParse(_inputPort.text, out portNo))
            {
                Debug.Log("VoiceChat :: 포트번호가 옳지않습니다");
                return;
            }

            _tcpRoom.StartTCPListener(portNo, OnStartedServer);
        }

        /// <summary>
        /// 서버 기동 완료
        /// </summary>
        public void OnStartedServer()
        {
            _startServer.gameObject.SetActive(false);
            _startedServer.gameObject.SetActive(true);
        }

        /// <summary>
        /// 서버 끊김
        /// </summary>
        public void OnDisconnectedServer()
        {
            _startServer.gameObject.SetActive(true);
            _startedServer.gameObject.SetActive(false);
        }

        /// <summary>
        /// 서버 서치허용
        /// </summary>
        public void StartDetectedServer()
        {
            StopAllCoroutines();

            int portNo = 0;
            if (!int.TryParse(_inputPort.text, out portNo))
            {
                Debug.Log("포트번호가 옳지않습니다");
                return;
            }
            _udpSender.ConnectBroadcastUDPClient(portNo);

            _isSearch = true;
            _searchWaitTime = 0;
            StartCoroutine(ISendDetectedPacket());
        }
        
        private IEnumerator ISendDetectedPacket()
        {
            _txtDetectedTime.gameObject.SetActive(true);

            var sendPacket = System.Text.Encoding.UTF8.GetBytes(PacketJsonConverter.JsonToString(new ServerDetected() { ServerIP = GetLocalIPv4() }));
            while (_isSearch)
            {
                yield return _wait;
                _searchWaitTime++;

                _txtDetectedTime.text = string.Format("{0}초남음", (100 - _searchWaitTime) / 10);
                _udpSender.SendPacket(sendPacket);

                if (_searchWaitTime > 100)
                    break;
            }

            OnEndDetectedServer();
        }

        public string GetLocalIPv4()
        {
            return Dns.GetHostEntry(Dns.GetHostName())
                .AddressList.First(
                    f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .ToString();
        }

        /// <summary>
        /// 서버 서치 끝
        /// </summary>
        public void OnEndDetectedServer()
        {

            _isSearch = false;
            _searchWaitTime = 0;
            _txtDetectedTime.gameObject.SetActive(false);
            _udpSender.StopUDPSender();

        }

        private void OnDestroy()
        {
            _tcpRoom.StopTCPListener();
            if(_isSearch)
                _udpSender.StopUDPSender();
        }
    }

}
