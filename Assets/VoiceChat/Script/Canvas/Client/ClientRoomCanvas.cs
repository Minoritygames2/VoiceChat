using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VoiceChat
{
    public class ClientRoomCanvas : MonoBehaviour
    {
        [SerializeField]
        private Text _txtServerIp;
        public void ActiveRoomArea(string serverIp)
        {
            _txtServerIp.text = serverIp;
        }
    }
}
