using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VoiceChat
{
    public class MicrophoneController : MonoBehaviour
    {
        public static string MicName_NONE = "None";
        [SerializeField]
        private AudioSource _micAudioSource;
        private string _settingMicName = string.Empty;

        private bool _isStartCapture = false;
        private WaitForSeconds _wait = new WaitForSeconds(10f);

        private int _voiceID = 0;
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

        ///// <summary>
        ///// 마이크 캡쳐 시작
        ///// </summary>
        //public void StartCapture(string micName, UnityAction<VoiceData> SendPacket)
        //{
        //    try
        //    {
        //        if (micName.Equals(MicName_NONE))
        //            return;
        //
        //        //마이크 설정이 되어있을 경우 끈다
        //        if (!_settingMicName.Equals(string.Empty))
        //        {
        //            Microphone.End(_settingMicName);
        //            _settingMicName = string.Empty;
        //        }
        //
        //        _settingMicName = micName;
        //
        //        Debug.Log("VoiceChat :: 마이크 캡쳐를 시작합니다" + micName);
        //
        //        _micAudioSource.clip = Microphone.Start(_settingMicName, true, 1, AudioSettings.outputSampleRate);
        //        _micAudioSource.loop = true;
        //        _micAudioSource.Play();
        //
        //        _isStartCapture = true;
        //
        //        StartCoroutine(SendVoice(SendPacket));
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.Log("VoiceChat :: 마이크 캡쳐 실패하였습니다 :: " + e.Message);
        //    }
        //}
        //
        //private IEnumerator SendVoice(UnityAction<VoiceData> SendPacket)
        //{
        //    while (_isStartCapture)
        //    {
        //        _voiceID++;
        //        if (_voiceID > 10000)
        //            _voiceID = 0;
        //
        //        var voiceValues = new float[44100];
        //        _micAudioSource.clip.GetData(voiceValues, Microphone.GetPosition(_settingMicName));
        //
        //        for (int index = 0; index < 3; index++)
        //        {
        //            var byteValue = new byte[voiceValues.Length * 4 / 3];
        //
        //            var byteValueIndex = 0;
        //            for (int voiceValueIndex = voiceValues.Length / 3 * index; voiceValueIndex < voiceValues.Length / 3 * (index + 1); voiceValueIndex++)
        //            {
        //                var data = BitConverter.GetBytes(voiceValues[voiceValueIndex]);
        //                Array.Copy(data, 0, byteValue, byteValueIndex, 4);
        //                byteValueIndex += 4;
        //            }
        //
        //            SendPacket?.Invoke(new VoiceData() { voiceID = _voiceID, voiceIndex = index, voiceArray = byteValue });
        //        }
        //        yield return _wait;
        //    }
        //
        //}

        /// <summary>
        /// 마이크 캡쳐 시작
        /// </summary>
        public void StartCapture(string micName, UnityAction<VoiceData> SendPacket)
        {
            try
            {
                if (micName.Equals(MicName_NONE))
                    return;

                //마이크 설정이 되어있을 경우 끈다
                if (!_settingMicName.Equals(string.Empty))
                {
                    Microphone.End(_settingMicName);
                    _settingMicName = string.Empty;
                }

                _settingMicName = micName;

                Debug.Log("VoiceChat :: 마이크 캡쳐를 시작합니다" + micName);
                _micAudioSource.loop = true;
                _micAudioSource.Play();

                _isStartCapture = true;

                StartCoroutine(SendVoice(SendPacket));
            }
            catch (Exception e)
            {
                Debug.Log("VoiceChat :: 마이크 캡쳐 실패하였습니다 :: " + e.Message);
            }
        }

        private IEnumerator SendVoice(UnityAction<VoiceData> SendPacket)
        {
            _voiceID++;
            if (_voiceID > 10000)
                _voiceID = 0;

            var voiceValues = new float[44100];
            _micAudioSource.clip.GetData(voiceValues, 0);

            for (int index = 0; index < 3; index++)
            {
                var byteValue = new byte[voiceValues.Length * 4 / 3];

                var byteValueIndex = 0;
                for (int voiceValueIndex = voiceValues.Length / 3 * index; voiceValueIndex < voiceValues.Length / 3 * (index + 1); voiceValueIndex++)
                {
                    var data = BitConverter.GetBytes(voiceValues[voiceValueIndex]);
                    Array.Copy(data, 0, byteValue, byteValueIndex, 4);
                    byteValueIndex += 4;
                }
                Debug.Log("SendPacket VoiceID : " + _voiceID + " voiceIndex : " + byteValue.Length);
                SendPacket?.Invoke(new VoiceData() { voiceID = _voiceID, voiceIndex = index, voiceArray = byteValue });
            }
            yield return _wait;

        }

    }

}
