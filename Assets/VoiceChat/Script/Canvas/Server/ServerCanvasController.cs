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
    }
}
