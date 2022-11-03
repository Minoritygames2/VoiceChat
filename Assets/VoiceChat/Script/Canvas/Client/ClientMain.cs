using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoiceChat
{
    public class ClientMain : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField]
        private ClientSettingController _settingCtrl;

        [SerializeField]
        private ClientRoomController _roomCtrl;

        [Header("Module")]
        [SerializeField]
        private MicrophoneController _micCtrl;

        [SerializeField]
        private UDPNetworkReceiver _udpReceiver;

        [SerializeField]
        private TCPNetworkClient _tcpClient;

        [Header("Room")]
        [SerializeField]
        private VoiceClientRoomCtrl _voiceClientRoom;

        private void Start()
        {
            StartSetting();
        }

        public void StartSetting()
        {
            _settingCtrl.ActiveSettingArea(
                //마이크 설정 버튼 클릭이벤트
                _micCtrl.SearchMicDevices,
                //서버 서치 버튼 클릭
                (port) =>
                {
                    _udpReceiver.StartUDPBroadcastReceiver(port, _settingCtrl.OnSearchedServer);
                },
                //서버 서치 타임아웃
                () => 
                {
                    _udpReceiver.StopUDPReceiver();
                },
                //세팅이 끝나면 불러오는 콜백
                StartVoiceChat);
        }

        public void StartVoiceChat(string micName, string ipAddress)
        {
            Debug.Log("세팅값 : MicName : " + micName + " ipAddress : " + ipAddress);
            //SettingArea 끄기
            _settingCtrl.UnactiveSettingArea();
            _roomCtrl.ActiveRoomArea(ipAddress);

            //TCP시작되면 마이크 캡쳐 시작
            _tcpClient.StartTcpClient(ipAddress,
                () => {
                    _micCtrl.StartCapture(micName, (voiceData) 
                        => { _tcpClient.SendVoicePacket(voiceData); });
                }
                , 
                (voiceData) => { _voiceClientRoom.SetVoiceData(voiceData); }
                );
        }
    }
}
