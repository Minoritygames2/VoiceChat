using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoiceChat
{
    public class VoiceClientRoomCtrl : MonoBehaviour
    {
        [SerializeField]
        private ClientRoomCanvas _roomCanvasCtrl;

        [Header("Network")]
        [SerializeField]
        private TCPClientSession _clientSession;

        [Header("MIC")]
        [SerializeField]
        private MicrophoneController _micCtrl;
        public void StartVoiceChat(string micName, string serverIp)
        {
            //VoiceChat Room Active상태로
            _roomCanvasCtrl.ActiveRoomArea(serverIp);

            //TCP시작되면 마이크 캡쳐 시작
            _clientSession.StartTcpClient(serverIp, micName);
        }
    }
}

