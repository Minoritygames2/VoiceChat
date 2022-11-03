using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VoiceChat
{
    public class BTN_ServerSearchItem : MonoBehaviour
    {
        [SerializeField]
        private Button _connectButton;
        [SerializeField]
        private Text _txtServerName;
        private string _serverName = string.Empty;
        public string GetServerName { get => _serverName; }
        public void SetButtonSetting(ServerDetected serverDetected, UnityAction<string> OnButtonTouched)
        {
            _serverName = serverDetected.ServerIP;
            _connectButton.onClick.AddListener(()=> {
                OnButtonTouched?.Invoke(_serverName);
            });
            _txtServerName.text = _serverName;
        }
    }
}
