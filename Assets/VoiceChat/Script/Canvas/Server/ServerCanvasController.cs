using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VoiceChat
{
    public class ServerCanvasController : MonoBehaviour
    {
        [SerializeField]
        private InputField _inputPort;
        [SerializeField, Header("StartServer")]
        private Button _btnStartServer;
        [SerializeField]
        private Transform _startServer;
        [SerializeField]
        private Transform _startedServer;
        [SerializeField, Header("DetectServer")]
        private Button _btnDetectedServer;
        [SerializeField]
        private Text _txtDetectedTime;
        [SerializeField, Header("Player")]
        private Transform _logInPlayerTransform;
        [SerializeField]
        private GameObject _canvasPlayerItemPrefab;
        [SerializeField]
        private Text _txtPlayerCount;

        public void InitButtons(UnityAction<string> onStartBtnClick, UnityAction<string> onDetectBtnClick)
        {
            _btnStartServer.onClick.AddListener(()=> {
                if (_inputPort.text.Equals(string.Empty))
                {
                    Debug.Log("VoiceChat :: 포트번호를 입력해주세요");
                    return;
                }
                onStartBtnClick?.Invoke(_inputPort.text);
            });
            _btnDetectedServer.onClick.AddListener(()=> {
                if (_inputPort.text.Equals(string.Empty))
                {
                    Debug.Log("VoiceChat :: 포트번호를 입력해주세요");
                    return;
                }
                onDetectBtnClick?.Invoke(_inputPort.text);
            });
        }

        #region 서버
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

        public void StartDetectServer()
        {
            _txtDetectedTime.gameObject.SetActive(true);
        }
        public void SetTimer(int time)
        {
            _txtDetectedTime.text = string.Format("{0}초남음", time);
        }
        public void OnEndDetectedServer()
        {
            _txtDetectedTime.gameObject.SetActive(false);
        }
        #endregion

        public CanvasPlayerItem CreateCanvasPlayerItem(int playerID)
        {
            var canvasPlayer = Instantiate(_canvasPlayerItemPrefab, _logInPlayerTransform).GetComponent<CanvasPlayerItem>();
            canvasPlayer.InitPlayerItem(playerID);
            return canvasPlayer;
        }

        public void ChangePlayerCount(int playerCount)
        {
            _txtPlayerCount.text = string.Format("{0}명 접속중", playerCount);
        }

    }
}
