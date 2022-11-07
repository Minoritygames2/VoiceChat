using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VoiceChat
{
    public class MicrophoneController : MonoBehaviour
    {
        /// <summary>
        /// 연결된 마이크 리스트 얻기
        /// </summary>
        public List<string> SearchMicDevices()
        {
            var rslt = new List<string>();
            foreach (var device in Microphone.devices)
            {
                Debug.Log("device : " + device);
                rslt.Add(device);
            }
            return rslt;
        }

    }

}
