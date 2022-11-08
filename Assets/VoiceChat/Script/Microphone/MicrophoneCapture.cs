using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VoiceChat
{
    public class MicrophoneCapture : MonoBehaviour
    {
        public static string MicName_NONE = "None";
        public static string MicName_TEST = "Test";

        [SerializeField]
        private AudioSource _micAudioSource;
        [SerializeField]
        private AudioClip _testAudioClip;
        private string _settingMicName = string.Empty;
        private string _changeMicName = string.Empty;

        private bool _isStartCapture = false;
        private WaitForSeconds _wait = new WaitForSeconds(0.1f);

        private int _voiceID = 0;

        public void SetChangeMicName(string micName)
        {
            _changeMicName = micName;
        }
        /// <summary>
        /// 마이크 캡쳐 시작
        /// </summary>
        public void StartCapture(UnityAction<VoiceData> SendPacket)
        {
            try
            {
                if (_changeMicName.Equals(MicName_NONE))
                    return;

                //마이크 설정이 되어있을 경우 끈다
                if (!_settingMicName.Equals(string.Empty))
                {
                    Microphone.End(_settingMicName);
                    _settingMicName = string.Empty;
                }

                _settingMicName = _changeMicName;

                Debug.Log("VoiceChat :: 마이크 캡쳐를 시작합니다" + _changeMicName);

                if (_changeMicName.Equals(MicName_TEST))
                    _micAudioSource.clip = _testAudioClip;
                else
                    _micAudioSource.clip = Microphone.Start(_settingMicName, true, 1, AudioSettings.outputSampleRate);
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
            while (_isStartCapture)
            {
                _voiceID++;
                if (_voiceID > 10000)
                    _voiceID = 0;

                var voiceValues = new float[24000];

                _micAudioSource.clip.GetData(voiceValues, _micAudioSource.timeSamples);

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
                    SendPacket?.Invoke(new VoiceData() { voiceID = _voiceID, voiceIndex = index, voiceArray = byteValue });
                }
                yield return _wait;
            }
        }
    }

}

