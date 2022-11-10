using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VoiceChat
{
    public class ClientRoomCanvas : MonoBehaviour
    {
        [SerializeField]
        private Transform _roomArea;
        [SerializeField]
        private Text _txtServerIp;
        [SerializeField]
        private Button _btnDisConnect;

        public void ActiveRoomArea(string serverIp, UnityAction onDisconnectBtnCliek)
        {
            _txtServerIp.text = serverIp;
            _roomArea.gameObject.SetActive(true);

            _btnDisConnect.onClick.AddListener(()=> {
                onDisconnectBtnCliek?.Invoke();
                _btnDisConnect.onClick.RemoveListener(onDisconnectBtnCliek);
            });
        }

        public void UnactiveRoomArea()
        {
            _txtServerIp.text = string.Empty;
            _roomArea.gameObject.SetActive(false);
        }
    }
}
