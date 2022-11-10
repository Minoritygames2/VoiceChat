using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoiceChat
{
    public class ServerMain : MonoBehaviour
    {
        [SerializeField]
        private ServerNetworkController _networkController;
        [SerializeField]
        private ServerCanvasController _canvasController;


        //서버 찾기 대기시간
        private WaitForSeconds _wait = new WaitForSeconds(0.1f);
        private bool _isSearch = false;
        private int _searchWaitTime = 0;

        private void Start()
        {
            _canvasController.InitButtons(StartNetwork, StartDetectServer);
        }

        public void StartNetwork(string portStr)
        {
            _networkController.StartServer(portStr, _canvasController.OnStartedServer);
        }

        /// <summary>
        /// 서버 서치 시작
        /// </summary>
        /// <param name="portStr"></param>
        public void StartDetectServer(string portStr)
        {
            _searchWaitTime = 0;

            _isSearch = true;

            _networkController.StartDetectedServer(portStr);
            StartCoroutine(ISendDetectedPacket());
        }

        /// <summary>
        /// 서버 서치 끝
        /// </summary>
        public void OnEndDetectedServer()
        {
            _isSearch = false;
            _searchWaitTime = 0;
            _networkController.StopUDPPacket();
        }

        private IEnumerator ISendDetectedPacket()
        {
            while (_isSearch)
            {
                yield return _wait;
                _searchWaitTime++;

                _canvasController.SetTimer((100 - _searchWaitTime) / 10);
                _networkController.SendUDPDetectPacket();
  
                if (_searchWaitTime > 100)
                    break;
            }

            OnEndDetectedServer();
        }
    }

}
