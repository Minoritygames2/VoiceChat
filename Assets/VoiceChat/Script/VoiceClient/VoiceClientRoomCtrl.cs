using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

        [Header("Player Canvas")]
        [SerializeField]
        private Transform _logInPlayerTransform;
        [SerializeField]
        private GameObject _canvasPlayerItemPrefab;
        public void StartVoiceChat(string micName, string serverIp, UnityAction onDisconnected)
        {
            //VoiceChat Room Active상태로
            _roomCanvasCtrl.ActiveRoomArea(serverIp, ()=> {
                StopVoiceChat();
                onDisconnected?.Invoke();
            });

            //TCP시작되면 마이크 캡쳐 시작
            _clientSession.StartTcpClient(serverIp, micName);
        }

        public void StopVoiceChat()
        {
            _clientSession.StopTcpClient();
            _roomCanvasCtrl.UnactiveRoomArea();
        }

        public CanvasPlayerItem CreateCanvasPlayerItem(int playerID)
        {
            var canvasPlayer = Instantiate(_canvasPlayerItemPrefab, _logInPlayerTransform).GetComponent<CanvasPlayerItem>();
            canvasPlayer.InitPlayerItem(playerID);
            return canvasPlayer;
        }
    }
}

